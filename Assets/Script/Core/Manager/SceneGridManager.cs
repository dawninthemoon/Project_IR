using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class SceneGridManager : MonoBehaviour
{
    public Grid mainGrid;
    public Tilemap mainWall;
    private void Awake()
    {
        SceneMaster.Instance().SetCharacterManager(this);
    }
}
