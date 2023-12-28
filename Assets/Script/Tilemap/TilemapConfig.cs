using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
[CreateAssetMenu(fileName = "Tilemap_NewTilemap", menuName = "ScriptableObjects/TilemapConfig")]
public class TilemapConfig : ScriptableObject {
    [System.Serializable]
    public struct TilemapInfo {
        public Vector3Int[] _positions;
        public TileBase[]   _tileBases;
        public TilemapInfo(Vector3Int[] positions, TileBase[] tileBases) {
            _positions = positions;
            _tileBases = tileBases;
        }
    }
    public string       _tilemapName;
    public TilemapInfo  _wall;
    public TilemapInfo  _background;
    
    public void Initialize(string tilemapName, TilemapInfo wall, TilemapInfo background) {
        _tilemapName = tilemapName;
        _wall = wall;
        _background = background;
    }
}
