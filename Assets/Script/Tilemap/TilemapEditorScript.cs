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
        private static readonly string WallTilemapKey = "Wall";
        private static readonly string ThroughPlatformTilemapKey = "ThroughPlatform";
        private static readonly string BackgroundTilemapKey = "Background";

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
            Tilemap backgroundTilemap = transform.Find(BackgroundTilemapKey).GetComponent<Tilemap>();
            backgroundTilemap.ClearAllTiles();
        }

        public void ClearAllWalls() 
        {
            Tilemap collisionTilemap = transform.Find(WallTilemapKey).GetComponent<Tilemap>();
            collisionTilemap.ClearAllTiles();
        }

        public void ClearAllThroughPlatforms()
        {
            Tilemap throughPlatformTilemap = transform.Find(ThroughPlatformTilemapKey).GetComponent<Tilemap>();
            throughPlatformTilemap.ClearAllTiles();
        }

        public void ClearAll()
        {
            ClearAllBackgrounds();
            ClearAllWalls();
            ClearAllThroughPlatforms();
        }

        public TilemapConfig LoadTilemapConfig(string tilemapName) 
        {
            Tilemap wallTilemap = transform.Find(WallTilemapKey).GetComponent<Tilemap>();
            Tilemap backgroundTilemap = transform.Find(BackgroundTilemapKey).GetComponent<Tilemap>();
            Tilemap throughPlatformTilemap = transform.Find(ThroughPlatformTilemapKey).GetComponent<Tilemap>();

            if (Config == null)
            {
                Config = ScriptableObject.CreateInstance<TilemapConfig>();
            }

            var wall = LoadTileInfo(wallTilemap);
            var background = LoadTileInfo(backgroundTilemap);
            var through = LoadTileInfo(throughPlatformTilemap);
            Config.Initialize(tilemapName, wall, background, through);

        #if UNITY_EDITOR
            EditorUtility.SetDirty(Config);
        #endif

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

            Tilemap collisionTilemap = transform.Find(WallTilemapKey).GetComponent<Tilemap>();
            Tilemap backgroundTilemap = transform.Find(BackgroundTilemapKey).GetComponent<Tilemap>();
            Tilemap throughPlatformTilemap = transform.Find(ThroughPlatformTilemapKey).GetComponent<Tilemap>();

            collisionTilemap.SetTiles(tilemapConfig._wall);
            backgroundTilemap.SetTiles(tilemapConfig._background);
            throughPlatformTilemap.SetTiles(tilemapConfig._throughPlatform);
        }
    }
}