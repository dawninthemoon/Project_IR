using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR

public class UnityWebRequestHelper : MonoBehaviour
{
    public static string _webHookDEV = "";
    public static string _webHookCHAT = "";

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
