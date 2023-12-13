using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtitleManager : Singleton<SubtitleManager>
{
    public Dictionary<string, SubtitleData_SimpleTalk> _simpleTalkData;

    public void SetSubtitleSimpleTalkData(Dictionary<string, SubtitleData_SimpleTalk> simpleTalkData)
    {
        _simpleTalkData = simpleTalkData;
    }

    public SubtitleData_SimpleTalk getSimpleTalkData(string key)
    {
        if(_simpleTalkData == null)
        {
            DebugUtil.assert(false,"Subtitle Info 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        if(_simpleTalkData.ContainsKey(key) == false)
            return null;

        return _simpleTalkData[key];
    }

}
