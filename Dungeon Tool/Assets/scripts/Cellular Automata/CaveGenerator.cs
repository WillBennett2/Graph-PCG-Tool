using UnityEngine;
using System;
using UnityEngine.Rendering.VirtualTexturing;

public class CaveGenerator : MonoBehaviour
{
    public int m_width;
    public int m_height;

    public string m_seed;
    public bool m_useRandomSeed = true;

    [Range(0, 100)] public int m_randomFillPercent;
    int[,] m_map;

    private void Start()
    {
        GenerateCave();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            GenerateCave();
    }

    void GenerateCave()
    {
        m_map = new int[m_width, m_height];
        RandomFillMap();

        for (int i = 0; i<5; i++) // 5 isnt concrete but depends on the cave shape
        {
            SmoothMap();
        }
    }

    void RandomFillMap()
    {
        if (m_useRandomSeed)
        {
            m_seed = System.DateTime.Now.ToString();
        }

        System.Random rand = new System.Random(m_seed.GetHashCode());

        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                if (x == 0 || x == m_width - 1 || y == 0 || y == m_height - 1) //sets a wall around the outside
                {
                    m_map[x, y] = 1;
                }
                else
                {
                    m_map[x, y] = (rand.Next(0, 100) < m_randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    m_map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    m_map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for(int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    if (neighbourX != gridX || neighbourY != gridY) //if not looking at self
                    {
                        wallCount += m_map[neighbourX, neighbourY]; //add its value to wallcount (0 is empty so it technically doesnt effect as its not a wall)
                        // change for alive and dead cells ? maybe
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    private void OnDrawGizmos()
    {
        if (m_map != null)
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    Gizmos.color = (m_map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 position = new Vector3(-m_width / 2 + x + 0.5f, 0, -m_height / 2 + y + 0.5f);
                    Gizmos.DrawCube(position, Vector3.one);
                }
            }
        }
    }
}
