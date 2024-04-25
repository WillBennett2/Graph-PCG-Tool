using UnityEngine;
using System;
using static Graph;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class CaveGenerator : MonoBehaviour
{
    public static event Action<int[,]> OnSetMapData;
    public static event Action<Vector3> OnSetTileData;
    //EntitySpawner m_entityScript;
    //TileMap m_tileMapGen;

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

    private GameObject m_roomContainer;
    private void OnEnable()
    {
        GraphComponent.OnClearData += Clear;
        GraphComponent.OnGenerateEnvrionment += GenerateCave;
    }
    private void OnDisable()
    {
        GraphComponent.OnClearData -= Clear;
        GraphComponent.OnGenerateEnvrionment -= GenerateCave;
    }
    public void Clear()
    {
        m_map = null;
        if(m_nodes!=null)
            m_nodes.Clear();
        if (m_edges != null)
            m_edges.Clear();
        foreach (GameObject room in m_createdRooms)
        {
            Destroy(room);
        }
        m_createdRooms.Clear();
    }

    public void GenerateCave(List<Index2NodeDataLinker> nodes, List<Index2EdgeDataLinker> edges, int width, int height, int offset, int scale)
    {
        
        SetUpFromGraph(nodes,edges,width,height,offset,scale);
        m_map = new int[m_width, m_height];
        ApplyGraphData();

        for (int i = 0; i < m_smoothIterations; i++)
        {
            SmoothMap();
        }
        Vector3 tileData;
        for (int x = 0; x < m_map.GetLength(0); x++)
        {
            for (int y = 0; y < m_map.GetLength(1); y++)
            {
                if (m_map[x, y] <= 0)
                {
                    tileData = new Vector3(x, 0, y);
                }
                else if(m_map[x, y] == 2)
                {
                    tileData = new Vector3(x, 5, y);
                }
                else if (m_map[x,y]==100)
                {
                    tileData = new Vector3(x, 100, y);
                }
                else
                {
                    tileData = new Vector3(x, 10, y);
                }
                OnSetTileData?.Invoke(tileData);
            }
        }

        OnSetMapData?.Invoke(m_map);
    }

    void ApplyGraphData()
    {
        m_roomContainer = new GameObject("Room holder");
        //Instantiate(m_roomContainer);

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
                m_nodes[i].nodeData.spaceWidth = m_nodes[i].nodeData.spaceHeight = randomDepth;
                if (m_useRandom)
                    SetRandomSurroundingCells((int)m_nodes[i].nodeData.position.x, (int)m_nodes[i].nodeData.position.z, randomDepth, rand);
                SetCaveDeadZones(i, m_scale, 2);
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
                        if (m_map[gridX, gridY] < 0 || 1 < m_map[gridX, gridY])
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
    void SetRandomSurroundingCells(int gridX, int gridY, int depth, System.Random rand)
    {
        for (int neighbourX = gridX - depth; neighbourX <= gridX + depth; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - depth; neighbourY <= gridY + depth; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    ChangeMapValue(neighbourX, neighbourY, (rand.Next(0, 100) < m_randomFillPercent) ? 1 : 0);
                }
            }
        }
    }
    void SetCaveDeadZones(int nodeIndex, int depth, int value)
    {
        if (m_nodes[nodeIndex].nodeData.upperEdge.index != -1
            && (m_nodes[nodeIndex].nodeData.upperEdge.edgeData.directional || m_nodes[nodeIndex].nodeData.upperEdge.edgeData.symbol == "edge"))
        {
            //create up blocker
            Vector3 posDifference = (m_nodes[nodeIndex].nodeData.upperEdge.edgeData.toPos + m_nodes[nodeIndex].nodeData.upperEdge.edgeData.fromPos) / 2;
            for (int posX = (int)posDifference.x - depth / 2; posX < posDifference.x + depth / 2; posX++)
            {
                ChangeMapValue(posX, (int)posDifference.z, value);
            }
        }
        if (m_nodes[nodeIndex].nodeData.rightEdge.index != -1
            && (m_nodes[nodeIndex].nodeData.rightEdge.edgeData.directional || m_nodes[nodeIndex].nodeData.rightEdge.edgeData.symbol == "edge"))
        {
            //create right blocker
            Vector3 posDifference = (m_nodes[nodeIndex].nodeData.rightEdge.edgeData.toPos + m_nodes[nodeIndex].nodeData.rightEdge.edgeData.fromPos) / 2;
            for (int posY = (int)posDifference.z - depth / 2; posY < posDifference.z + depth / 2; posY++)
            {
                ChangeMapValue((int)posDifference.x, posY, value);
            }
        }
        if (nodeIndex - m_graphWidth > 0)
        {
            if (m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.index != -1
                && (m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.directional || m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.symbol == "edge"))
            {
                //create left blocker
                Vector3 posDifference = (m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.toPos + m_nodes[nodeIndex - m_graphWidth].nodeData.rightEdge.edgeData.fromPos) / 2;
                for (int posY = (int)posDifference.z - depth / 2; posY < posDifference.z + depth / 2; posY++)
                {
                    ChangeMapValue((int)posDifference.x, posY, value);
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
                    ChangeMapValue(posX, (int)posDifference.z, value);
                }
            }
        }
    }

    void SetRoomCells(int gridX, int gridY, Index2NodeDataLinker node, int value)
    {
        int roomWidth=0, roomHeight =0;
        
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
                        roomWidth = room.m_roomWidth/2;
                        roomHeight = room.m_roomHeight/2;
                        roomPrefab = room.m_roomPrefab;
                        GameObject roomObject = Instantiate(roomPrefab, new Vector3(gridX + 0.5f, 0, gridY + 0.5f), Quaternion.identity, m_roomContainer.transform);
                        m_createdRooms.Add(roomObject);
                        break;

                    }
                }
            }
        }
        node.nodeData.spaceHeight = roomHeight;
        node.nodeData.spaceWidth = roomWidth;

        //SETTING WALL BOUNDRY
        for (int neighbourX = gridX - roomWidth; neighbourX <= gridX + roomWidth; neighbourX++) //LOOPS ROUND XY COORD
        {
            for (int neighbourY = gridY - roomHeight; neighbourY <= gridY + roomHeight; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height) //within the grid
                {
                    //check for outer edge of depth set to extreme pos
                    if(neighbourX == gridX - roomWidth || neighbourX == gridX + roomWidth || neighbourY == gridY - roomHeight || neighbourY == gridY + roomHeight)
                    {
                        if(0 < m_map[neighbourX, neighbourY])
                            m_map[neighbourX, neighbourY] = 2;
                        if(m_map[neighbourX, neighbourY] != 100)
                        {
                            if (gridX == neighbourX && gridY + roomHeight == neighbourY)
                                SetUpDoor(node, neighbourX, neighbourY, roomRef);
                            if (gridX + roomWidth == neighbourX && gridY == neighbourY)
                                SetRightDoor(node, neighbourX, neighbourY, roomRef);
                            if (gridX == neighbourX && gridY - roomHeight == neighbourY)
                                SetDownDoor(node, neighbourX, neighbourY, roomRef);
                            if (gridX - roomWidth == neighbourX && gridY == neighbourY)
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

    bool CheckNeighbouringRooms(Index2NodeDataLinker node, int posX, int posY)
    {
        bool canSpawn = true;

        //up
        if (m_nodes[node.nodeData.upperEdge.edgeData.toNode].nodeData.preAuthored)
        {
            //get neighbouring nodes symbol and check the room sizes if one is smaller then change it to that
            //check against SO if neighbouring room reaches this room
            return false;
        }
        //right
        if (m_nodes[node.nodeData.rightEdge.edgeData.toNode].nodeData.preAuthored)
        {
            return false;
        }
        //down
        if (0 < node.index - 1)
        {
            if (m_nodes[m_nodes[node.index - 1].nodeData.upperEdge.edgeData.toNode].nodeData.preAuthored)
            {
                return false;
            }
        }
        //left
        if (0 < node.index - m_graphWidth)
        {
            if (m_nodes[m_nodes[node.index - m_graphWidth].nodeData.rightEdge.edgeData.toNode].nodeData.preAuthored)
            {
                return false;
            }
        }
        return canSpawn;
    }

    void SetUpDoor(Index2NodeDataLinker node, int posX, int posY, PreAuthoredRoomSO.Room roomRef)
    {
        if (node.nodeData.upperEdge.edgeData.terrian <= 0)
            m_map[posX, posY] = -1;
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX+0.5f, 0, posY), Quaternion.identity,m_roomContainer.transform);
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
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX, 0, posY+0.5f), Quaternion.identity, m_roomContainer.transform);
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
                GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX + 0.5f, 0, posY + 1f), Quaternion.identity, m_roomContainer.transform);
                m_createdRooms.Add(blockerObj);
            }
        }
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX + 0.5f, 0, posY + 1f), Quaternion.identity, m_roomContainer.transform);
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
                GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX+1f, 0, posY+0.5f), Quaternion.identity, m_roomContainer.transform);
                m_createdRooms.Add(blockerObj);
            }
        }
        else
        {
            GameObject blockerObj = Instantiate(roomRef.m_roomDoorBlockerPrefab, new Vector3(posX + 1f, 0, posY + 0.5f), Quaternion.identity, m_roomContainer.transform);
            m_createdRooms.Add(blockerObj);
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
