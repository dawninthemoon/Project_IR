using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public ActionGraphConditionCompareData compareData;
    public CharacterEntityBase character;
    public string targetFormula="(TargetDistance > 0.7) && true";
        ActionGraph graph = new ActionGraph();

    void Start()
    {
        //compareData = ActionGraphLoader.ReadConditionCompareData(targetFormula);
    }

    void Update()
    {
        Debug.Log(graph.processActionCondition(compareData));

    }
}
