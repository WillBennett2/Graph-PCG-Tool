using UnityEngine;
using System;
using UnityEngine.Rendering.VirtualTexturing;
using static Graph;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

public class CaveGenerator : MonoBehaviour
{
    public int m_width;
    public int m_height;
    public int m_borderSize = 1;

    public string m_seed;
    public bool m_useRandomSeed = true;
    public bool m_useRandom = true;

    [Range(0, 100)] public int m_randomFillPercent;
    [SerializeField] private int m_smoothIterations = 5;
    [SerializeField][Min(1)] private int m_depth = 1;
    [SerializeField] private int m_roomSize = 2;


    int[,] m_map;
    private List<Index2NodeDataLinker> m_nodes;
    private List<Index2EdgeDataLinker> m_edges;
    private void Start()
    {
        m_roomSize *= m_depth;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            GenerateCave();
    }

    public void GenerateCave()
    {
        m_map = new int[m_width, m_height];
        RandomFillMap();

        for (int i = 0; i< m_smoothIterations; i++) // 5 isnt concrete but depends on the cave shape
        {
            SmoothMap();
        }

        int[,] borderedMap = new int[m_width + m_borderSize * 2, m_height + m_borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= m_borderSize && x < m_width + m_borderSize && y >= m_borderSize && y < m_height + m_borderSize) // within the map
                {
                    borderedMap[x, y] = m_map[x - m_borderSize, y - m_borderSize];
                }
                else
                    borderedMap[x, y] = 1;
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1,m_borderSize);
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
                    if(m_borderSize>0)
                        m_map[x, y] = 1;
                }
                else
                { 
                    if(m_useRandom)
                        m_map[x, y] = (rand.Next(0, 100) < m_randomFillPercent) ? 1 : 0;
                    else
                        m_map[x, y] = 1;

                    for (int i = 0; i < m_nodes.Count; i++)
                    {
                        if (m_nodes[i].nodeData.position.x == x && m_nodes[i].nodeData.position.z == y)
                        {
                            if(m_nodes[i].nodeData.preAuthored)
                            {
                                SetSurroundingCells(x, y, 1);
                            }
                            else if (m_nodes[i].nodeData.terrain <= 0)
                            {
                                m_map[x, y] = m_nodes[i].nodeData.terrain;
                                //SetSurroundingCells(x, y, m_depth);
                            }
                        }
                    }

                    for (int i = 0; i < m_edges.Count; i++)
                    {
                        if (m_edges[i].edgeData.fromPos.x == x)// check if x pos matches
                        {

                            for (int pathY = (int)m_edges[i].edgeData.fromPos.z; pathY <= m_edges[i].edgeData.toPos.z; pathY++)
                            {
                                if (m_edges[i].edgeData.terrian <= 0)
                                {
                                    m_map[x, pathY] = m_edges[i].edgeData.terrian;
                                    //SetSurroundingCells(x, pathY, m_depth);
                                }
                            }
                        }
                        if (m_edges[i].edgeData.fromPos.z == y)// check if x pos matches
                        {
                            for (int pathX = (int)m_edges[i].edgeData.fromPos.x; pathX <= m_edges[i].edgeData.toPos.x; pathX++)
                            {
                                if (m_edges[i].edgeData.terrian <= 0)
                                {
                                    m_map[pathX, y] = m_edges[i].edgeData.terrian;
                                    //SetSurroundingCells(pathX, y, m_depth);
                                }
                            }
                        }

                    }
                }
            }
        }

    }

    public void PopulateFromGraph(List<Index2NodeDataLinker> nodes,List<Index2EdgeDataLinker> edges)
    {
        m_nodes = nodes;
        m_edges = edges;
    }

    void SmoothMap()
    {
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                //if (m_map[x,y]<0)
                //{
                //    break;
                //}
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

    void SetSurroundingCells(int gridX, int gridY, int depth)
    {
        for (int neighbourX = gridX - depth; neighbourX <= gridX + depth; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - depth; neighbourY <= gridY + depth; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    Debug.Log("checking [" + neighbourX + "," + neighbourY + "]");
                    if(neighbourX != gridX || neighbourY != gridY) // not looking at self
                    {
                        m_map[neighbourX, neighbourY] = -5;
                        Debug.Log("checked [" + neighbourX + "," + neighbourY + "]");
                    }
                }
            }
        }
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
