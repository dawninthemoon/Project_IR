using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSearchDescription : MessageData
{
    public ObjectBase              _requester;
    public TargetSearchType        _searchType;
    public SearchIdentifier        _searchIdentifier;
    public float                   _searchRange;
    public float                   _searchStartRange;
    public float                   _searchSphereRadius;
}

public struct SpawnCharacterOptionDesc
{
    public Vector3          _position;

    public Vector3          _direction;
    public Quaternion       _rotation;
    public SearchIdentifier _searchIdentifier;

    public static SpawnCharacterOptionDesc defaultValue = new SpawnCharacterOptionDesc
    { 
        _position = Vector3.zero, 
        _direction = Vector3.right, 
        _rotation = Quaternion.identity, 
        _searchIdentifier = SearchIdentifier.Count
    };
}

public class SpawnCharacterOptionDescData : MessageData
{
    public SpawnCharacterOptionDesc _spawnCharacterOptionDesc;
    public CharacterInfoData _characterInfoData;
}



public class SceneCharacterManager : ManagerBase
{
    public delegate void OnCharacterEnabled(CharacterEntityBase character);
    public delegate void OnCharacterDisabled(CharacterEntityBase character);

    [System.Serializable]
    private struct CharacterKeyValue
    {
        public string name;
        public int uniqueID;
        public bool IsValid() {return name != null && name.Equals("") == false && uniqueID != 0;}
    }

    private Dictionary<int, string> _characterIDCacheMap = new Dictionary<int, string>();

    private Dictionary<int, CharacterEntityBase> _enableCharacterPoolCacheMap = new Dictionary<int, CharacterEntityBase>();
    private Queue<CharacterEntityBase> _disableCharacterPoolCacheMap = new Queue<CharacterEntityBase>();

    private List<TargetSearchDescription> _targetSearchRequestList = new List<TargetSearchDescription>();

    private OnCharacterEnabled _onCharacterEnabled = (character)=>{};
    private OnCharacterDisabled _onCharacterDisabled = (character)=>{};

    public override void RegisterReceiver(ObjectBase receiver)
    {
        base.RegisterReceiver(receiver);
        _characterIDCacheMap.Add(receiver.GetUniqueID(),receiver.gameObject.name);
    }

    public override void DeregisteReceiver(int target)
    {
        ObjectBase targetReciver = GetReciever(target);
        if(targetReciver != null && targetReciver is CharacterEntityBase)
        {
            CharacterEntityBase characterEntity = targetReciver as CharacterEntityBase;
            
            CollisionManager.Instance().deregisterObject(characterEntity.getCollisionInfo().getCollisionInfoData(),characterEntity);

            if(_enableCharacterPoolCacheMap.ContainsKey(target))
            {
                _disableCharacterPoolCacheMap.Enqueue(_enableCharacterPoolCacheMap[target]);
                _enableCharacterPoolCacheMap.Remove(target);

                _onCharacterDisabled(characterEntity);
            }
        }

        _characterIDCacheMap.Remove(target);
        base.DeregisteReceiver(target);
    }

    public override void assign()
    {
        base.assign();
        CacheUniqueID("SceneCharacterManager");
        RegisterRequest();
        SceneMaster.Instance().SetCharacterManager(this);

        AddAction(MessageTitles.entity_searchNearest,(msg)=>{
            TargetSearchDescription desc = msg.data as TargetSearchDescription;
            _targetSearchRequestList.Add(desc);
        });

        AddAction(MessageTitles.entity_searchNearestQuick,(msg)=>{
            TargetSearchDescription desc = msg.data as TargetSearchDescription;
            processTargetSearchQuick(desc);
        });

        AddAction(MessageTitles.entity_spawnCharacter, (msg)=>{
            SpawnCharacterOptionDescData desc = MessageDataPooling.CastData<SpawnCharacterOptionDescData>(msg.data);
            createCharacterFromPool(desc._characterInfoData, desc._spawnCharacterOptionDesc);
        });

        AddAction(MessageTitles.game_stageEnd, (msg)=>{
            deregisterAll();
        });
    }

    public override void initialize()
    {
        base.initialize();

    }

    public override void progress(float deltaTime)
    {
        processTargetSearch();
        base.progress(deltaTime);
    }

   
    public void processTargetSearch()
    {
        int requestCount = _targetSearchRequestList.Count;
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
                continue;

            for(int i = 0; i < requestCount; ++i)
            {
                TargetSearchDescription desc = _targetSearchRequestList[i];

                updateTargetSearch(receiver, desc);
            }
        }

        for(int i = 0; i < requestCount; ++i)
        {
            MessageDataPooling.ReturnData(_targetSearchRequestList[i]);
        }
        _targetSearchRequestList.Clear();
    }

    public void processTargetSearchQuick(TargetSearchDescription desc)
    {
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
                continue;

            updateTargetSearch(receiver, desc);
        }

        MessageDataPooling.ReturnData(desc);
    }

    public bool targetSearchRange(Vector3 centerPosition, float searchRange, SearchIdentifier searchIdentifier, ref List<CharacterEntityBase> outCharacterList)
    {
        if(searchIdentifier == SearchIdentifier.Count)
        {
            DebugUtil.assert(false,"invalid search identifier: Count");
            return false;
        }

        foreach(var character in _enableCharacterPoolCacheMap.Values)
        {
            if(character._searchIdentifier != searchIdentifier || character.gameObject.activeInHierarchy == false || character.enabled == false || character.isDead())
                continue;
            
            if(Vector3.Distance(centerPosition, character.transform.position) > searchRange)
                continue;

            outCharacterList.Add(character);
        }

        return true;
    }


    private bool updateTargetSearch(ObjectBase receiver, TargetSearchDescription desc)
    {
        float range = desc._searchRange * desc._searchRange;

        if(desc._requester is CharacterEntityBase == false || receiver is CharacterEntityBase == false)
        {
            DebugUtil.assert(false,"must be character entity, code error");
            return false;
        }
        else if(desc._searchIdentifier == SearchIdentifier.Count)
        {
            DebugUtil.assert(false,"invalid search identifier: Count");
            return false;
        }

        CharacterEntityBase requester = desc._requester as CharacterEntityBase;
        CharacterEntityBase receiverCharacter = receiver as CharacterEntityBase;

        GameEntityBase currentTarget = requester.getCurrentTargetEntity();
        float toNewDistanceSq = receiverCharacter.getDistanceSq(requester);

        if(targetIsValid(requester,currentTarget,desc) == false)
            requester.setTargetEntity(null);

        switch(desc._searchType)
        {
            case TargetSearchType.Near:
            {
                if(requester == receiverCharacter || desc._searchIdentifier != receiverCharacter._searchIdentifier || receiverCharacter.isDead())
                    return true;

                if(currentTarget == null)
                {
                    if(toNewDistanceSq < range)
                        requester.setTargetEntity(receiverCharacter);
                    return true;
                }

                float toCurrent = currentTarget.getDistanceSq(requester);
                if(toNewDistanceSq < range && toCurrent > toNewDistanceSq)
                    requester.setTargetEntity(receiverCharacter);
            }
            break;
            case TargetSearchType.NearDirection:
            case TargetSearchType.NearMousePointDirection:
            {
                if(requester == receiverCharacter || desc._searchIdentifier != receiverCharacter._searchIdentifier || receiverCharacter.isDead())
                    return true;

                Vector3 direction = Vector3.right;
                if(desc._searchType == TargetSearchType.NearDirection)
                    direction = requester.getDirection();
                else if(desc._searchType == TargetSearchType.NearMousePointDirection)
                    direction = requester.getDirectionFromType(DirectionType.MousePoint);

                if(MathEx.pointSphereRayCast(requester.transform.position + direction * requester.getCurrentTargetSearchStartRange()
                            , receiverCharacter.transform.position,direction * desc._searchRange
                            , desc._searchSphereRadius) == false)
                {
                    return true;
                }

                if(currentTarget == null)
                {
                    if(toNewDistanceSq < range)
                        requester.setTargetEntity(receiverCharacter);
                    return true;
                }

                bool currentTargetValid = MathEx.pointSphereRayCast(requester.transform.position + direction * requester.getCurrentTargetSearchStartRange()
                            , currentTarget.transform.position,direction * desc._searchRange
                            , desc._searchSphereRadius);

                float toCurrent = currentTarget.getDistanceSq(requester);
                if(toNewDistanceSq < range && (toCurrent > toNewDistanceSq || currentTargetValid == false))
                    requester.setTargetEntity(receiverCharacter);
            }
            break;

        }

        return true;
    }

    public bool targetIsValid(GameEntityBase currentCharacter, GameEntityBase targetCharacter, TargetSearchDescription searchDesc)
    {
        if(targetCharacter == null || targetCharacter.gameObject.activeInHierarchy == false)
            return false;

        float range = searchDesc._searchRange * searchDesc._searchRange;

        switch(searchDesc._searchType)
        {
            case TargetSearchType.Near:
            {
                return currentCharacter.getDistanceSq(targetCharacter) < range;
            }
            case TargetSearchType.NearDirection:
            case TargetSearchType.NearMousePointDirection:
            {
                Vector3 direction = Vector3.right;
                if(searchDesc._searchType == TargetSearchType.NearDirection)
                    direction = currentCharacter.getDirection();
                else if(searchDesc._searchType == TargetSearchType.NearMousePointDirection)
                    direction = currentCharacter.getDirectionFromType(DirectionType.MousePoint);

                bool currentTargetIsValid = MathEx.pointSphereRayCast(
                            currentCharacter.transform.position + direction * currentCharacter.getCurrentTargetSearchStartRange()
                            , targetCharacter.transform.position
                            , direction * searchDesc._searchRange
                            , searchDesc._searchSphereRadius);

                return currentCharacter.getDistanceSq(targetCharacter) < range && currentTargetIsValid;
            }
        }

        return false;
    }

    public CharacterEntityBase createCharacterFromPool(CharacterInfoData characterData, SpawnCharacterOptionDesc spawnDesc)
    {
        CharacterEntityBase characterEntity = null;
        if(_disableCharacterPoolCacheMap.Count != 0)
        {
            characterEntity = _disableCharacterPoolCacheMap.Dequeue();
        }
        else
        {
            GameObject characterObject = new GameObject(characterData._displayName);
            characterObject.layer = LayerMask.NameToLayer("Character");
            characterObject.AddComponent<GroundController>();
            characterEntity = characterObject.AddComponent<CharacterEntityBase>();
        }

        characterEntity.gameObject.SetActive(true);

        _enableCharacterPoolCacheMap.Add(characterEntity.GetUniqueID(), characterEntity);

        characterEntity._searchIdentifier = spawnDesc._searchIdentifier;
        characterEntity.transform.position = spawnDesc._position;
        characterEntity.transform.rotation = spawnDesc._rotation;

        characterEntity.initializeCharacter(characterData,spawnDesc._direction);
        
        _onCharacterEnabled(characterEntity);
        return characterEntity;
    }

    public Dictionary<int,CharacterEntityBase> getCurrentlyEnabledCharacters()
    {
        return _enableCharacterPoolCacheMap;
    }

    public GameEntityBase GetCharacter(string targetName)
    {
        int key = FindCharacterKey(targetName);
        if(IsInReceivers(key) == false)
        {
            DebugUtil.assert(false,"target not found : {0}, {1}",targetName, key);
            return null;
        }
        return GetReciever(key) as GameEntityBase;
    }

    private int FindCharacterKey(string targetName)
    {
        foreach(var character in _characterIDCacheMap)
        {
            if(character.Value.CompareTo(targetName) == 0)
            {
                return character.Key;
            }
        }
        DebugUtil.assert(false,"attempt to find an invalid target: {0}",targetName);
        return -1;
    }

    public void addCharacterEnableDelegate(OnCharacterEnabled enabled)
    {
        _onCharacterEnabled += enabled;
    }

    public void deleteCharacterEnableDelegate(OnCharacterEnabled enabled)
    {
        _onCharacterEnabled -= enabled;
    }

    public void addCharacterDisableDelegate(OnCharacterDisabled disabled)
    {
        _onCharacterDisabled += disabled;
    }

    public void deleteCharacterDisableDelegate(OnCharacterDisabled disabled)
    {
        _onCharacterDisabled -= disabled;
    }
}
