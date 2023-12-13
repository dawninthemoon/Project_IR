using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TilemapConfig : ScriptableObject {
    public string       _tilemapName;
    public Vector3Int[] _positions;
    public TileBase[]   _tileBases;

    public void Initialize(string tilemapName, Vector3Int[] positions, TileBase[] tileBases) {
        _tilemapName = tilemapName;
        _positions = positions;
        _tileBases = tileBases;
    }
}
