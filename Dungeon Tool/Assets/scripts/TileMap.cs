using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TileMap : MonoBehaviour
{

    [SerializeField] bool m_debugMode = false;
    [SerializeField] Tilemap tileMap;
    [SerializeField] Tilemap upperMap;
    [SerializeField] Tile m_tile;
    [SerializeField] GameObject higher;
    [SerializeField] GameObject lower;
    [SerializeField] GameObject limiter;

    // Start is called before the first frame update
    private void OnEnable()
    {
        GraphComponent.OnClearData += Clear;
        CaveGenerator.OnSetTileData += SetTile;
    }
    private void OnDisable()
    {
        GraphComponent.OnClearData -= Clear;
        CaveGenerator.OnSetTileData -= SetTile;
    }

    void Start()
    {
        m_tile = ScriptableObject.CreateInstance<Tile>();
    }

    GameObject GetGameObject(int yPos)
    {
        GameObject gameObject;
        if (yPos <= 0)
            gameObject = lower;
        else
            gameObject = higher;
        if (m_debugMode)
        {
            if (yPos == 5)
            {
                gameObject = limiter;
            }
        }

        return gameObject; 
    }
    public void SetTile(Vector3 tiledata)
    {
        if (tiledata.y == 100)
        {
            return;
        }

        Vector3Int currentCell;

        if (tiledata.y <= 0)
        {
            currentCell = tileMap.WorldToCell(tiledata);
            m_tile.gameObject = GetGameObject((int)tiledata.y);
            tileMap.SetTile(currentCell, m_tile);
        }        
        else
        {
            currentCell = upperMap.WorldToCell(tiledata);
            m_tile.gameObject = GetGameObject((int)tiledata.y);
            upperMap.SetTile(currentCell, m_tile);
        }

        if (m_debugMode)
        {
            if (tiledata.y == 5)
            {

                currentCell = upperMap.WorldToCell(tiledata);
                m_tile.gameObject = GetGameObject((int)tiledata.y);
                upperMap.SetTile(currentCell, m_tile);
            }
        }
    }

    public void Clear()
    {
        tileMap.ClearAllTiles();
        upperMap.ClearAllTiles();
    }
}
