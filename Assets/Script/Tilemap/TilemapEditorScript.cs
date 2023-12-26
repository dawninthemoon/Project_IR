using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RieslingUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TilemapEditor {
    [ExecuteInEditMode]
    public class TilemapEditorScript : MonoBehaviour
    {
        public TilemapConfig Config
        {
            get;
            private set;
        }

        private void Start()
        {
            ClearAll();
        }

        public void ClearAllBackgrounds() 
        {
            Tilemap backgroundTilemap = transform.Find("Background").GetComponent<Tilemap>();
            backgroundTilemap.ClearAllTiles();
        }

        public void ClearAllWalls() 
        {
            Tilemap collisionTilemap = transform.Find("Wall").GetComponent<Tilemap>();
            collisionTilemap.ClearAllTiles();
        }

        public void ClearAll()
        {
            ClearAllBackgrounds();
            ClearAllWalls();
        }

        public TilemapConfig LoadTilemapConfig(string tilemapName) 
        {
            Tilemap wallTilemap = transform.Find("Wall").GetComponent<Tilemap>();
            Tilemap backgroundTilemap = transform.Find("Background").GetComponent<Tilemap>();

            if (Config == null)
            {
                Config = ScriptableObject.CreateInstance<TilemapConfig>();
            }

            var wall = LoadTileInfo(wallTilemap);
            var background = LoadTileInfo(backgroundTilemap);
            Config.Initialize(tilemapName, wall, background);

            EditorUtility.SetDirty(Config);

            return Config;
        }

        private TilemapConfig.TilemapInfo LoadTileInfo(Tilemap tilemap) 
        {
            List<Vector3Int> positions = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();

            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
                if (!tilemap.HasTile(pos)) continue;
                positions.Add(pos);
                tileBases.Add(tilemap.GetTile(pos));
            }

            return new TilemapConfig.TilemapInfo(positions.ToArray(), tileBases.ToArray());
        }

        public void Import(TilemapConfig tilemapConfig) 
        {
            ClearAll();

            Config = tilemapConfig;

            Tilemap collisionTilemap = transform.Find("Wall").GetComponent<Tilemap>();
            Tilemap backgroundTilemap = transform.Find("Background").GetComponent<Tilemap>();

            collisionTilemap.SetTiles(tilemapConfig._wall);
            backgroundTilemap.SetTiles(tilemapConfig._background);
        }
    }
}