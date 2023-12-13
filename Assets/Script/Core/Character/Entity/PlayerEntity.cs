using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : GameEntityBase
{
    public static ushort hidePlayerEvent = MessageTitles.customTitle_start;
    public override void assign()
    {
        base.assign();
        CacheUniqueID("Player");
        AddAction(hidePlayerEvent,(msg=>{
            GetComponent<SpriteRenderer>().enabled = (bool)msg.data;
        }));
    }
}