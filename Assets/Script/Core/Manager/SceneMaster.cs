using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneMaster : Singleton<SceneMaster>
{
    private SceneCharacterManager       _characterManager;
    private SceneGridManager            _gridManager;

    public GameEntityBase GetSceneCharacter(string targetName)
    {
        return _characterManager.GetCharacter(targetName);
    }
    public UnityEngine.Tilemaps.Tilemap GetWallTilemap()
    {
        return _gridManager.mainWall;
    }
    public Grid GetMainGrid()
    {
        return _gridManager.mainGrid;
    }
    public void SetGridAndWall(Grid grid, UnityEngine.Tilemaps.Tilemap wall)
    {
        _gridManager.mainGrid = grid;
        _gridManager.mainWall = wall;
    }
    public void SetCharacterManager(SceneGridManager gridManager)
    {
        _gridManager = gridManager;
    }
    public void SetCharacterManager(SceneCharacterManager characterManager)
    {
        _characterManager = characterManager;
    }
}
