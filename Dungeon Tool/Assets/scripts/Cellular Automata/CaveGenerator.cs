using UnityEngine;
using System;
using static Graph;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class CaveGenerator : MonoBehaviour
{
    private int m_width;
    private int m_height;
    private int m_scale;
    private int m_graphWidth;
    private int m_graphHeight;
    public int m_borderSize = 1;

    [Header("CA values")]
    public string m_seed;
    public bool m_useRandomSeed = true;
    public bool m_useRandom = true;
    [Range(0, 100)] public int m_randomFillPercent;
    [SerializeField] private int m_smoothIterations = 5;

    [Header("Cave values")]
    [SerializeField][Min(1)] private int m_depth = 1;
    [Tooltip("Equal values to not use random")]
    [SerializeField][Min(1)] private int m_randomNodeDepthMin = 1;
    [SerializeField][Min(1)] private int m_randomNodeDepthMax = 1;
    [Header("Pre-authored Rooms")]
    [SerializeField] private PreAuthoredRoomSO m_roomSets;
    private List<GameObject> m_createdRooms = new List<GameObject>();


    int[,] m_map;
    private List<Index2NodeDataLinker> m_nodes;
    private List<Index2EdgeDataLinker> m_edges;
    public void Clear()
    {
        m_map = null;
        m_nodes.Clear();
        m_edges.Clear();
        foreach (GameObject room in m_createdRooms)
        {
            Destroy(room);
        }
        m_createdRooms.Clear();
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

        for (int i = 0; i < m_smoothIterations; i++) // 5 isnt concrete but depends on the cave shape
        {
            SmoothMap();
        }

        TileMap tileMapGen = GetComponent<TileMap>();
        for (int x = 0; x < m_map.GetLength(0); x++)
        {
            for (int y = 0; y < m_map.GetLength(1); y++)
            {
                if (m_map[x, y] <= 0)
                {
                    tileMapGen.SetTile(x, 0, y);
                }
                else if(m_map[x, y] == 2)
                {
                    tileMapGen.SetTile(x, 5, y);
                }
                else if (m_map[x,y]==100)
                {
                    tileMapGen.SetTile(x, 100, y);
                }
                else
                {
                    tileMapGen.SetTile(x, 10, y);
                }
            }
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
                m_map[x, y] = 1;
                if (x == 0 || x == m_width - 1 || y == 0 || y == m_height - 1) //sets a wall around the outside
                {
                    if (m_borderSize > 0)
                        m_map[x, y] = 1;
                }
                else
                {
                    for (int i = 0; i < m_edges.Count; i++)
                    {
                        if (m_edges[i].edgeData.fromPos.x == x)
                        {

                            for (int pathY = (int)m_edges[i].edgeData.fromPos.z; pathY <= m_edges[i].edgeData.toPos.z; pathY++)
                            {
                                if (m_edges[i].edgeData.terrian <= 0)
                                {
                                    ChangeMapValue(x, pathY, m_edges[i].edgeData.terrian);
                                }
                            }
                        }
                        if (m_edges[i].edgeData.fromPos.z == y)
                        {
                            for (int pathX = (int)m_edges[i].edgeData.fromPos.x; pathX <= m_edges[i].edgeData.toPos.x; pathX++)
                            {
                                if (m_edges[i].edgeData.terrian <= 0)
                                {
                                    ChangeMapValue(pathX, y, m_edges[i].edgeData.terrian);
                                }
                            }
                        }

                    }
                }
            }
        }

        for (int i = 0; i < m_nodes.Count; i++)
        {
            if (m_nodes[i].nodeData.preAuthored)
            {
                m_map[(int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.z] = 100;
                SetRoomCells((int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.z, m_nodes[i], -2);

            }
            else if (m_nodes[i].nodeData.terrain <= 0)
            {
                m_map[(int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.z] = m_nodes[i].nodeData.terrain;
                SetSurroundingCells((int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.z, m_depth, -1);
                int randomDepth = Random.Range(m_randomNodeDepthMin, m_randomNodeDepthMax);
                if (m_useRandom)
                    SetRandomSurroundingCells((int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.z, randomDepth, 0, rand);
                SetCaveDeadZones((int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.y, i, m_scale, 2);
            }

        }
    }

    public void SetUpFromGraph(List<Index2NodeDataLinker> nodes,List<Index2EdgeDataLinker> edges, int width, int height,int offset,int scale)
    {
        m_nodes = nodes;
        m_edges = edges;
        m_width = ((width + offset) * (scale));
        m_height = ((height + offset) * (scale));
        m_scale = scale;

        m_graphWidth = width;
        m_graphHeight = height;

    }

    void SmoothMap()
    {
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                if (m_map[x, y] < 0 || 1 < m_map[x,y])
                {
                    continue;
                }
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
                        if (m_map[neighbourX, neighbourY] == -2)
                            break;
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

    void SetSurroundingCells(int gridX, int gridY, int depth, int value)
    {
        for (int neighbourX = gridX - depth; neighbourX <= gridX + depth; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - depth; neighbourY <= gridY + depth; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    if(neighbourX != gridX || neighbourY != gridY) // not looking at self
                    {
                        m_map[neighbourX, neighbourY] = value;
                    }
                }
            }
        }
    }
    void SetRandomSurroundingCells(int gridX, int gridY, int depth, int value, System.Random rand)
    {
        for (int neighbourX = gridX - depth; neighbourX <= gridX + depth; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - depth; neighbourY <= gridY + depth; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    for(int i =0; i< depth; i++)
                    {
                        if (neighbourX == gridX - depth -i || neighbourX == gridX + depth - i 
                            || neighbourY == gridY - depth - i || neighbourY == gridY + depth - i)
                        {
                            ChangeMapValue(neighbourX, neighbourY, (rand.Next(0, 100) < m_randomFillPercent) ? 1 : 0);
                        }
                    }
                }
            }
        }
    }

    void SetRoomCells(int gridX, int gridY, Index2NodeDataLinker node, int value)
    {
        int roomSize=0;
        GameObject roomPrefab;
        PreAuthoredRoomSO.Room roomRef = new PreAuthoredRoomSO.Room();
        System.Random rand = new System.Random();
        //find correct wall
        foreach (var roomSet in m_roomSets.m_roomSets) 
        {
            if (roomSet.m_alphabetKey == node.nodeData.symbol)
            {
                foreach (PreAuthoredRoomSO.Room room in roomSet.m_roomPrefab)
                {
                    if (rand.Next(0, 100) <= room.m_chanceOfAppearing)
                    {
                        //get random room based on chance of appearing
                        roomRef = room;
                        roomSize = room.m_roomSize;
                        roomPrefab = room.m_roomPrefab;
                        GameObject roomObject = Instantiate(roomPrefab, new Vector3(gridX + 0.5f, 0, gridY + 0.5f), Quaternion.identity);
                        m_createdRooms.Add(roomObject);
                        break;

                    }
                }
            }
        }

        //SETTING WALL BOUNDRY

        for (int neighbourX = gridX - roomSize; neighbourX <= gridX + roomSize; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - roomSize; neighbourY <= gridY + roomSize; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    //check for outer edge of depth set to extreme pos
                    if(neighbourX == gridX - roomSize || neighbourX == gridX + roomSize || neighbourY == gridY - roomSize || neighbourY == gridY + roomSize)
                    {
                        if(0 < m_map[neighbourX, neighbourY])
                            m_map[neighbourX, neighbourY] = 2;
                        if(m_map[neighbourX, neighbourY] != 100)
                        {
                            if (gridX == neighbourX && gridY + roomSize == neighbourY)
                                SetUpDoor(node, neighbourX, neighbourY, roomRef);
                            if (gridX + roomSize == neighbourX && gridY == neighbourY)
                                SetRightDoor(node, neighbourX, neighbourY, roomRef);
                            if (gridX == neighbourX && gridY - roomSize == neighbourY)
                                SetDownDoor(node, neighbourX, neighbourY, roomRef);
                            if (gridX - roomSize == neighbourX && gridY == neighbourY)
                                SetLeftDoor(node, neighbourX, neighbourY, roomRef);
                        }
                    }
                    else if (neighbourX != gridX || neighbourY != gridY) // not looking at self
                    {
                        if (0 < m_map[neighbourX, neighbourY] || m_map[neighbourX, neighbourY]==-1)
                            m_map[neighbourX, neighbourY] = 100;
                    }
                }
            }
        }
    }

    void SetUpDoor(Index2NodeDataLinker node, int posX, int posY, PreAuthoredRoomSO.Room roomRef)
    {
        if (node.nodeData.upperEdge.edgeData.terrian <= 0)
            m_map[posX, posY] = -1;
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX+0.5f, 0, posY), Quaternion.identity);
            m_createdRooms.Add(blockerObj);
        }
    }
    void SetRightDoor(Index2NodeDataLinker node, int posX, int posY, PreAuthoredRoomSO.Room roomRef)
    {
        if (node.nodeData.rightEdge.edgeData.terrian <= 0)
        {
            m_map[posX, posY] = -1;
        }
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX, 0, posY+0.5f), Quaternion.identity);
            m_createdRooms.Add(blockerObj);
        }
    }
    void SetDownDoor(Index2NodeDataLinker node, int posX, int posY, PreAuthoredRoomSO.Room roomRef)
    {
        if (0 < node.index - 1)
        {
            if (m_nodes[node.index - 1].nodeData.upperEdge.edgeData.terrian <= 0)
            {
                m_map[posX, posY] = -1;
            }
            else
            {
                GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX + 0.5f, 0, posY + 1f), Quaternion.identity);
                m_createdRooms.Add(blockerObj);
            }
        }
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX + 0.5f, 0, posY + 1f), Quaternion.identity);
            m_createdRooms.Add(blockerObj);
        }
    }
    void SetLeftDoor(Index2NodeDataLinker node, int posX, int posY, PreAuthoredRoomSO.Room roomRef)
    {
        if (0 < node.index - m_graphWidth)
        {
            if (m_nodes[node.index - m_graphWidth].nodeData.rightEdge.edgeData.terrian <= 0)
            {
                m_map[posX, posY] = -1;
            }
            else
            {
                GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX+1f, 0, posY+0.5f), Quaternion.identity);
                m_createdRooms.Add(blockerObj);
            }
        }
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX + 1f, 0, posY + 0.5f), Quaternion.identity);
            m_createdRooms.Add(blockerObj);
        }
    }

    void SetCaveDeadZones(int gridX, int gridY, int nodeIndex, int depth, int value)
    {
        /* check 4 directions of nodes if neighbouring nodes are rooms or not
        if node is non directional cave then no divide between
        if node is directional cave then divider between

        if room/cave then barrier
        if room/room then smoosh rooms to touch (wall between)
        */

        if (m_nodes[nodeIndex].nodeData.upperEdge.index != -1 
            && (m_nodes[nodeIndex].nodeData.upperEdge.edgeData.directional || m_nodes[nodeIndex].nodeData.upperEdge.edgeData.symbol == "edge"))
        {
            //create up blocker
            Vector3 posDifference = (m_nodes[nodeIndex].nodeData.upperEdge.edgeData.toPos + m_nodes[nodeIndex].nodeData.upperEdge.edgeData.fromPos) / 2;
            for (int posX = (int)posDifference.x - depth / 2; posX < posDifference.x + depth / 2; posX++)
            {
                //m_map[posX,(int)posDifference.z] = value;
                ChangeMapValue(posX, (int)posDifference.z,value);
            }
        }
        if (m_nodes[nodeIndex].nodeData.rightEdge.index != -1 
            && (m_nodes[nodeIndex].nodeData.rightEdge.edgeData.directional || m_nodes[nodeIndex].nodeData.rightEdge.edgeData.symbol == "edge"))
        {
            //create right blocker
            Vector3 posDifference = (m_nodes[nodeIndex].nodeData.rightEdge.edgeData.toPos + m_nodes[nodeIndex].nodeData.rightEdge.edgeData.fromPos)/2;
            for (int posY = (int)posDifference.z-depth/2; posY < posDifference.z+depth/2; posY++)
            {
                //m_map[(int)posDifference.x,posY] = value;
                ChangeMapValue((int)posDifference.x, posY,value);
            }
        }
        if (nodeIndex - m_graphWidth > 0)
        {
            if (m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.index != -1 
                && (m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.directional|| m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.symbol == "edge"))
            {
                //create left blocker
                Vector3 posDifference = (m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.toPos + m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.fromPos) / 2;
                for (int posY = (int)posDifference.z - depth / 2; posY < posDifference.z + depth / 2; posY++)
                {
                    //m_map[(int)posDifference.x, posY] = value;
                    ChangeMapValue((int)posDifference.x, posY,value);
                }
            }
        }
        if (nodeIndex - 1 > 0)
        {
            if (m_nodes[nodeIndex - 1].nodeData.upperEdge.index != -1
                && (m_nodes[nodeIndex - 1].nodeData.upperEdge.edgeData.directional || m_nodes[nodeIndex - 1].nodeData.upperEdge.edgeData.symbol == "edge"))
            {
                //create down blocker
                Vector3 posDifference = (m_nodes[nodeIndex - 1].nodeData.upperEdge.edgeData.toPos + m_nodes[nodeIndex - 1].nodeData.upperEdge.edgeData.fromPos) / 2;
                for (int posX = (int)posDifference.x - depth / 2; posX < posDifference.x + depth / 2; posX++)
                {
                    //m_map[posX, (int)posDifference.z] = value;
                    ChangeMapValue(posX, (int)posDifference.z,value);
                }
            }
        }
    }

    private void ChangeMapValue(int posX,int posY, int value)
    {
        if ( 0 == m_map[posX, posY] || m_map[posX,posY]==1)
        {
            m_map[posX, posY] = value;
        }
    }
}
