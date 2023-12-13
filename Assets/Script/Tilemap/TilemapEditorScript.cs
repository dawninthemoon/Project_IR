using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapEditor {
    [ExecuteInEditMode]
    public class TilemapEditorScript : MonoBehaviour
    {
        public void ClearAll() {
            ClearAllTilemaps();
        }

        public void ClearAllTilemaps() {
            Tilemap collisionTilemap = transform.Find("Collisionable").GetComponent<Tilemap>();

            collisionTilemap.ClearAllTiles();
        }

        public TilemapConfig RequestExport(string tilemapName) {
            Tilemap collisionableTilemap = transform.Find("Collisionable").GetComponent<Tilemap>();

            TilemapConfig asset = ScriptableObject.CreateInstance<TilemapConfig>();
            LoadTileInfo(tilemapName, collisionableTilemap, asset);

            return asset;
        }

        private void LoadTileInfo(string tilemapName, Tilemap tilemap, TilemapConfig asset) {
            List<Vector3Int> positions = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();

            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
                if (!tilemap.HasTile(pos)) continue;
                positions.Add(pos);
                tileBases.Add(tilemap.GetTile(pos));
            }

            asset.Initialize(tilemapName, positions.ToArray(), tileBases.ToArray());
        }

        public void Import(TilemapConfig tilemap) {
            ClearAll();

            Tilemap collisionTilemap = transform.Find("Collisionable").GetComponent<Tilemap>();

            collisionTilemap.SetTiles(tilemap._positions, tilemap._tileBases);
        }
    }
}