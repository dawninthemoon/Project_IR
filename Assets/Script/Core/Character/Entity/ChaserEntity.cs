// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ChaserEntity : GameEntityBase
// {
//     public string           targetKey = "";
//     ChaserEntityControlEx   _control;
//     GameEntityBase          _targetCharacter = null;
//     public override void assign()
//     {
//         base.assign();
//     }
//     public override void initialize()
//     {
//         base.initialize();
//     }
//     public override void firstUpdate()
//     {
//         base.firstUpdate();
//         _targetCharacter = SceneMaster.Instance().GetSceneCharacter(targetKey);
//         _control = getControlManager().SetControl<ChaserEntityControlEx>(this.transform);
//         _control.SetChaseTarget(_targetCharacter.transform,5f,6f);
//     }
//     public override void progress(float deltaTime)
//     {
//         base.progress(deltaTime);
//     }
//     public override void release()
//     {
//         base.release();
//     }
//     protected override void OnTriggerEnter2D(Collider2D coll)
//     {
//         base.OnTriggerEnter2D(coll);
//     }
// }
