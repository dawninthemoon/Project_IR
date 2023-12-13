using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;

public class EffectInfoManager : Singleton<EffectInfoManager>
{
    public Dictionary<string, EffectInfoDataBase[]> _effectInfoData;

    public void setEffectInfoData(Dictionary<string, EffectInfoDataBase[]> effectInfoData)
    {
        _effectInfoData = effectInfoData;
    }

    public EffectInfoDataBase[] getEffectInfoData(string key)
    {
        if(_effectInfoData == null)
        {
            DebugUtil.assert(false,"Effect Info 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        if(_effectInfoData.ContainsKey(key) == false)
            return null;

        return _effectInfoData[key];
    }

    public bool requestEffect(string effectInfoKey, GameEntityBase executeEntity, ObjectBase targetEntity, CommonMaterial attackMaterial)
    {
        CommonMaterial defenceMaterial = CommonMaterial.Empty;
        if(targetEntity != null && targetEntity is GameEntityBase)
            defenceMaterial = (targetEntity as GameEntityBase).getCharacterMaterial();
        
        EffectRequestData requestData = createRequestData(effectInfoKey,executeEntity,targetEntity,attackMaterial, defenceMaterial);
        if(requestData == null)
            return false;

        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);
        return true;
    }

    public EffectRequestData createRequestData(string key, ObjectBase executeEntity, ObjectBase targetEntity, CommonMaterial attackMaterial, CommonMaterial defenceMaterial)
    {
        EffectInfoDataBase[] infoArray = getEffectInfoData(key);
        if(infoArray == null)
            return null;

        foreach(var item in infoArray)
        {
            if(item.compareMaterial(attackMaterial, defenceMaterial))
                return item.createEffectRequestData(executeEntity,targetEntity);
        }

        return null;
    }


}
