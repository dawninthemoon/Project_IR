using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR

public class UnityWebRequestHelper : MonoBehaviour
{
    public static string _webHookDEV = "https://discord.com/api/webhooks/1184750217128579072/DW1RllvCwwZRCInJodpQXzbJ4sME2siRcjBJHKK45-wxnxceTGeZZ-stKImE0L_dXviS";
    public static string _webHookCHAT = "https://discord.com/api/webhooks/1184748879250477117/5iiKwYoCK3o8nWDMRySLxzl8Ha4TzgYyduAYmrDPN7-liFIxd48FPhxQQp6TLjhVt0h2";

    public void sendWebHook(string webHook, string message)
    {
        StartCoroutine(sendWebHookInner(webHook, message));
    }

    private IEnumerator sendWebHookInner(string link, string message)
    {
        if(link != "")
        {
            WWWForm form = new WWWForm();
            form.AddField("content", message);
            using (UnityWebRequest request = UnityWebRequest.Post(link,form))
            {
                yield return request.SendWebRequest();

                if(request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError )
                {
                    UnityEngine.Debug.Log("Web Requset Error : " + request.error);
                }
            }
        }
        
        DestroyImmediate(gameObject);
    }


}
#endif
