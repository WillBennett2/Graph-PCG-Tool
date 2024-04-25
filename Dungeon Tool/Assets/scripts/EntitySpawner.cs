using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Graph;
using Random = UnityEngine.Random;

public class EntitySpawner : MonoBehaviour
{
    int[,] m_map;
    int[,] m_PDEntityMap;
    int[,] m_JGEntityMap;

    bool m_usePoisson;
    bool m_useJitter;

    List<GameObject> m_entities = new List<GameObject>();

    private List<Index2NodeDataLinker> m_nodes;
    private List<Index2StoredNodeDataLinker> m_storedNodes;

    private GameObject m_entityContainer;
    private void OnEnable()
    {
        GraphComponent.OnClearData += ClearData;
        GraphComponent.OnSpawnEntities += SetData;
        CaveGenerator.OnSetMapData += SetMapData;
    }
    private void OnDisable()
    {
        GraphComponent.OnClearData -= ClearData;
        GraphComponent.OnSpawnEntities -= SetData;
        CaveGenerator.OnSetMapData -= SetMapData;
    }

    public void SetData(List<Index2NodeDataLinker> nodes, List<Index2StoredNodeDataLinker> storedNodes,bool usePoisson, bool useJitter)
    {
        m_entityContainer = new GameObject("EntityHolder");
        m_nodes = nodes;
        m_storedNodes = storedNodes;
        m_usePoisson = usePoisson;
        m_useJitter = useJitter;

        CreateEntity();
    }
    public void SetMapData(int[,] map)
    {
        m_map = new int[map.GetLength(0), map.GetLength(1)];
        m_PDEntityMap = new int[map.GetLength(0), map.GetLength(1)];
        m_JGEntityMap = new int[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                m_map[x,y] = map[x,y];
                m_PDEntityMap[x, y] = 0;
                m_JGEntityMap[x, y] = 0;
            }
        }
    }

    public void CreateEntity()
    {
        for (int index = 0; index < m_storedNodes.Count; index++)
        {
            InstantiateEntity(index,(int)m_nodes[m_storedNodes[index].storedNodeData.parentIndex].nodeData.position.x, (int)m_nodes[m_storedNodes[index].storedNodeData.parentIndex].nodeData.position.z);
        }

    }

    private void InstantiateEntity(int index,int posX, int posY)
    {
        System.Random rand = new System.Random();
        foreach (EntitySpawnSetSO.EntitySet entitySet in m_storedNodes[index].storedNodeData.entitySet.m_entitySets)
        {
            if (rand.Next(0, 100) <= entitySet.m_chanceOfAppearing)
            {
                foreach (EntitySpawnSetSO.Entity entity in entitySet.m_entities)
                {
                    Vector2 position = new Vector2(-1, -1);
                    if (m_usePoisson)
                        position = GetPoissonEmptySpace(entity, m_nodes[m_storedNodes[index].storedNodeData.parentIndex].nodeData, posX, posY);
                    else if(m_useJitter)
                        position = GetJitterEmptySpace(entity, m_nodes[m_storedNodes[index].storedNodeData.parentIndex].nodeData, posX, posY);
                    if (position.x != -1 && position.y != -1)
                    { 
                        GameObject entityRef = Instantiate(entity.m_entityPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity, m_entityContainer.transform);
                        m_entities.Add(entityRef);
                    }
                }
                break;
            }

        }
    }

    private Vector2 GetPoissonEmptySpace(EntitySpawnSetSO.Entity entity, NodeData node,int posX, int posY)
    {
        int depthHeight = node.spaceHeight*2;
        int depthWidth = node.spaceWidth*2;

        List<int> roomMap= new List<int>();

        for (int i = 0; i < depthHeight * depthHeight; i++)
        {
            roomMap.Add(i);
        }

        for (int i =0; i< depthHeight * depthHeight; i++)
        {
            int index = Random.Range(0, roomMap.Count - 1);
            int randomXPos = index % depthWidth;
            int randomYPos = index / depthWidth;
            roomMap.RemoveAt(index);

            randomXPos += posX-node.spaceWidth;
            randomYPos += posY-node.spaceHeight;

            if (randomXPos < 0 || randomYPos < 0)
                continue;
            if (m_map[randomXPos, randomYPos] >= 1 && m_map[randomXPos, randomYPos] != 100)
                continue;
            if (m_PDEntityMap[randomXPos, randomYPos] == 1)
                continue;

            //free space
            bool surroundingClear = true;
            for (int x = randomXPos - (entity.m_widthOfEntity/2); x < randomXPos + (entity.m_widthOfEntity / 2); x++)
            {
                for (int y = randomYPos - (entity.m_lengthOfEntity/2); y < randomYPos + (entity.m_lengthOfEntity / 2); y++)
                {
                    if(y<0 || x<0 || x >= m_PDEntityMap.GetLength(0) || y >= m_PDEntityMap.GetLength(1))//within bounds
                        continue;

                    if (0 < m_map[x, y] && m_map[x, y] != 100)//bound reach edge?
                    {
                        surroundingClear = false;
                    }

                    if (m_PDEntityMap[x, y] == 1)
                    {
                        surroundingClear = false;
                        break;
                    }
                }
            }
            if (surroundingClear)
            {
                m_PDEntityMap[randomXPos, randomYPos] = 1;
                return new Vector2(randomXPos, randomYPos);
            }
            
        }
        return new Vector2(-1, -1);
    }

    private Vector2 GetJitterEmptySpace(EntitySpawnSetSO.Entity entity, NodeData node, int posX, int posY)
    {
        int depthHeight = node.spaceHeight * 2;
        int depthWidth = node.spaceWidth * 2;

        List<int> roomMap = new List<int>();

        for (int i = 0; i < depthHeight * depthHeight; i++)
        {
            roomMap.Add(i);
        }

        for (int i = 0; i < depthHeight * depthHeight; i++)
        {
            int index = Random.Range(0, roomMap.Count - 1);
            int randomXPos = index % depthWidth;
            int randomYPos = index / depthWidth;
            roomMap.RemoveAt(index);

            randomXPos += posX - node.spaceWidth;
            randomYPos += posY - node.spaceHeight;
            //free space
            //apply jitter
            int jitterPosX = (int)(randomXPos + (Mathf.Sin(randomXPos)*2));
            int jitterPosY = (int)(randomYPos + (Mathf.Cos(randomYPos)*2));

            //free space
            if (jitterPosX < 0 || posX + node.spaceWidth < jitterPosX
                || jitterPosY < 0 || posY + node.spaceHeight < jitterPosY)
                continue;
            if (1 <= m_map[jitterPosX, jitterPosY] && m_map[jitterPosX, jitterPosY] != 100)
                continue;
            if (m_JGEntityMap[jitterPosX, jitterPosY] == 1)
                continue;
            bool clearSpawn = true;
            for (int x = jitterPosX - (entity.m_widthOfEntity/2); x < jitterPosX + (entity.m_widthOfEntity / 2); x++)
            {
                for (int y = jitterPosY - (entity.m_lengthOfEntity/2); y < jitterPosY + (entity.m_lengthOfEntity / 2); y++)
                {
                    if (y < 0 || x < 0 || x >= m_JGEntityMap.GetLength(0) || y >= m_JGEntityMap.GetLength(1))//within bounds
                    {
                        clearSpawn = false;
                        continue;
                    }

                    if (1 <= m_map[x, y] && m_map[x, y] != 100)//bound reach edge?
                    {
                        clearSpawn = false;
                        continue;
                    }
                }
            }
            if (clearSpawn)
            {
                Debug.Log("normal pos = " + randomXPos + "," + randomYPos + " jitt = " + jitterPosX + "," + jitterPosY);
                m_JGEntityMap[jitterPosX, jitterPosY] = 1;
                return new Vector2(jitterPosX, jitterPosY);
            }
        }
        return new Vector2(-1,-1);
    }

    public void ClearData()
    {
        m_map = null;
        m_PDEntityMap = null;
        m_JGEntityMap = null;
        m_nodes=null;
        m_storedNodes = null;
        foreach (GameObject entity in m_entities)
        {
            Destroy(entity);
        }
        m_entities.Clear();
    }

}
