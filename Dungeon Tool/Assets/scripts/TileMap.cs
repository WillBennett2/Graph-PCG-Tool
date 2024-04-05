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
    public void SetTile(int xPos, int yPos, int zPos)
    {
        //Debug.Log("set tile");
        if (yPos ==100)
        {
            return;
        }
        Vector3Int tilePos = new Vector3Int(xPos, yPos, zPos);
        Vector3Int currentCell;// = tileMap.WorldToCell(tilePos);


        if (yPos <= 0)
        {
            currentCell = tileMap.WorldToCell(tilePos);
            m_tile.gameObject = GetGameObject(yPos);
            tileMap.SetTile(currentCell, m_tile);
        }        
        else
        {
            currentCell = upperMap.WorldToCell(tilePos);
            m_tile.gameObject = GetGameObject(yPos);
            upperMap.SetTile(currentCell, m_tile);
        }

        if (m_debugMode)
        {
            if (yPos == 5)
            {

                currentCell = upperMap.WorldToCell(tilePos);
                m_tile.gameObject = GetGameObject(yPos);
                upperMap.SetTile(currentCell, m_tile);
            }
        }

        //m_tile.gameObject = GetGameObject();

        //tileMap.SetTile(new Vector3Int(0, 0, 0), m_tile);
    }

    public void Clear()
    {
        tileMap.ClearAllTiles();
        upperMap.ClearAllTiles();
    }
}
