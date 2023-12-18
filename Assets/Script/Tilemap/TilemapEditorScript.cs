using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RieslingUtils;

namespace TilemapEditor {
    [ExecuteInEditMode]
    public class TilemapEditorScript : MonoBehaviour
    {
        public void ClearAllBackgrounds() 
        {
            Tilemap backgroundTilemap = transform.Find("Background").GetComponent<Tilemap>();
            backgroundTilemap.ClearAllTiles();
        }

        public void ClearAllTilemaps() 
        {
            Tilemap collisionTilemap = transform.Find("Wall").GetComponent<Tilemap>();
            collisionTilemap.ClearAllTiles();
        }

        public TilemapConfig RequestExport(string tilemapName) 
        {
            Tilemap wallTilemap = transform.Find("Wall").GetComponent<Tilemap>();
            Tilemap backgroundTilemap = transform.Find("Background").GetComponent<Tilemap>();

            TilemapConfig asset = ScriptableObject.CreateInstance<TilemapConfig>();
            var wall = LoadTileInfo(wallTilemap);
            var background = LoadTileInfo(backgroundTilemap);
            asset.Initialize(tilemapName, wall, background);

            return asset;
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
            ClearAllBackgrounds();

            Tilemap collisionTilemap = transform.Find("Wall").GetComponent<Tilemap>();
            Tilemap backgroundTilemap = transform.Find("Background").GetComponent<Tilemap>();

            collisionTilemap.SetTiles(tilemapConfig._wall);
            collisionTilemap.SetTiles(tilemapConfig._background);
        }
    }
}