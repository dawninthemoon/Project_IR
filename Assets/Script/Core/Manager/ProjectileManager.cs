using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedProjectileItem
{
    public float _timer;
    public string _graphName = "";
    public ProjectileGraphShotInfoData _shotInfo;
    public SearchIdentifier _searchIdentifier;
    public ObjectBase _executeEntity;
    public ObjectBase _targetEntity;

    public ObjectBase _summoner;

    public SetTargetType _sethTargetType;

    public bool updateTimer(float deltaTime)
    {
        _timer -= deltaTime;
        return _timer <= 0f;
    }
}

public class ProjectileManager : PoolingManagerBase<ProjectileEntityBase>
{
    public static ProjectileManager _instance;
    public string _projectileGraphPath = "";

    private Queue<ProjectileEntityBase> _projectilePool = new Queue<ProjectileEntityBase>();
    private SimplePool<DelayedProjectileItem> _delayedProjectileItemPool = new SimplePool<DelayedProjectileItem>();

    private List<DelayedProjectileItem> _currentUpdateList = new List<DelayedProjectileItem>();
    private Dictionary<string,ProjectileGraphBaseData> _projectileGraphDataList = new Dictionary<string, ProjectileGraphBaseData>();

    public override void assign()
    {
        _instance = this;

        base.assign();
        CacheUniqueID("ProjectileManager");
        RegisterRequest();
        
        _projectileGraphDataList.Clear();
        ProjectileGraphBaseData[] graphDataList = ResourceContainerEx.Instance().GetProjectileGraphBaseData(IOControl.PathForDocumentsFile(_projectileGraphPath));

        if(graphDataList == null)
        {
            DebugUtil.assert(false, "projectileGraphBaseData load fail: {0}",_projectileGraphPath);
            return;
        }

        for(int i = 0; i < graphDataList.Length; ++i)
        {
            _projectileGraphDataList.Add(graphDataList[i]._name, graphDataList[i]);
        }
        
    }

    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);

        for(int index = 0; index < _currentUpdateList.Count; ++index)
        {
            if(_currentUpdateList[index].updateTimer(deltaTime))
            {
                Vector3 spawnPosition = ActionFrameEvent_Projectile.getSpawnPosition(_currentUpdateList[index]._sethTargetType,_currentUpdateList[index]._executeEntity,_currentUpdateList[index]._targetEntity);
                spawnProjectile(_currentUpdateList[index]._graphName,ref _currentUpdateList[index]._shotInfo,spawnPosition,_currentUpdateList[index]._summoner,_currentUpdateList[index]._searchIdentifier);

                _delayedProjectileItemPool.enqueue(_currentUpdateList[index]);
                _currentUpdateList.RemoveAt(index);
                --index;
            }

        }
    }

    public void spawnProjectileDelayed(string name, float time, ObjectBase executeEntity, ObjectBase targetEntity, SetTargetType setTargetType, ref ProjectileGraphShotInfoData shotInfo, ObjectBase summoner, SearchIdentifier searchIdentifier)
    {
        DelayedProjectileItem delayedProjectileItem = _delayedProjectileItemPool.dequeue();
        delayedProjectileItem._executeEntity = executeEntity;
        delayedProjectileItem._targetEntity = targetEntity;
        delayedProjectileItem._summoner = summoner;
        delayedProjectileItem._graphName = name;
        delayedProjectileItem._shotInfo = shotInfo;
        delayedProjectileItem._timer = time;
        delayedProjectileItem._sethTargetType = setTargetType;

        _currentUpdateList.Add(delayedProjectileItem);
    }

    public void spawnProjectile(string name, ref ProjectileGraphShotInfoData shotInfo, Vector3 startPosition, ObjectBase summoner, SearchIdentifier searchIdentifier)
    {
        ProjectileEntityBase entity = dequeuePoolEntity();

        entity._searchIdentifier = searchIdentifier;
        entity.setSummonObject(summoner);
        entity.setData(getProjectileGraphData(name));
        entity.initialize();
        entity.shot(shotInfo,startPosition);
    }

    public void spawnProjectile(string name, Vector3 startPosition, ObjectBase summoner, SearchIdentifier searchIdentifier)
    {
        ProjectileEntityBase entity = dequeuePoolEntity();

        entity._searchIdentifier = searchIdentifier;
        entity.setSummonObject(summoner);
        entity.setData(getProjectileGraphData(name));
        entity.initialize();
        entity.shot(startPosition);
    }

    public ProjectileGraphBaseData getProjectileGraphData(string name)
    {
        if(_projectileGraphDataList.ContainsKey(name) == false)
        {
            DebugUtil.assert(false, "projectileGraphBaseData is not exists : {0}",name);
            return null;
        }

        return _projectileGraphDataList[name];
    }
}
