using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class SequencerObjectBase : ObjectBase
{
    // [SerializeField] private EventSequencer eventSequencer;
    // public override void Assign()
    // {
    //     base.Assign();
    //     AddAction(MessageTitles.contents_ownSequencePlayRequest,(msg)=>{
    //         PlayEventSequencerSingle();
    //     });
    // }
    // public override void Initialize()
    // {
    //     RegisterRequest(QueryUniqueID("SceneCharacterManager"));
    // }
    // public virtual void PlayEventSequencerSingle() 
    // {
    //     if(eventSequencer == null)
    //     {
    //         DebugUtil.assert(false,"event sequence is missing");
    //         return;
    //     }
        
    //     eventSequencer.PlayEventSequencerAsync();
    // }
    // public virtual void PlayEventSequencerTarget(SequencerObjectBase target)
    // {
    //     //eventSequencer.
    // }
}
