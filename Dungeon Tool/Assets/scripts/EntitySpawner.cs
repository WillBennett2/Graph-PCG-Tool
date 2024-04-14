using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Graph;
using Random = UnityEngine.Random;

public class EntitySpawner : MonoBehaviour
{
    int[,] m_entityMap;
    List<GameObject> m_entities = new List<GameObject>();
    int[,] m_map;

    private List<Index2NodeDataLinker> m_nodes;
    private List<Index2StoredNodeDataLinker> m_storedNodes;

    public void SetGraphData(List<Index2NodeDataLinker> nodes, List<Index2StoredNodeDataLinker> storedNodes)
    {
        m_nodes = nodes;
        m_storedNodes = storedNodes;
    }
    public void SetMapData(int[,] map)
    {
        m_map = new int[map.GetLength(0), map.GetLength(1)];
        m_entityMap = new int[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                m_map[x,y] = map[x,y];
                m_entityMap[x, y] = 0;
                
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
                    Vector2 position = GetEmptySpace(entity, m_nodes[m_storedNodes[index].storedNodeData.parentIndex].nodeData, posX, posY);
                    if (position.x != -1 && position.y != -1)
                    { 
                        GameObject entityRef = Instantiate(entity.m_entityPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
                        m_entities.Add(entityRef);
                    }
                }
            }

        }
    }

    private Vector2 GetEmptySpace(EntitySpawnSetSO.Entity entity, NodeData node,int posX, int posY)
    {
        int depthHeight = node.spaceHeight;
        int depthWidth = node.spaceWidth;

        for (int i =0; i<=depthHeight*depthWidth;i++)
        {
            int randomXPos = Random.Range(posX - depthWidth, posX + depthWidth);
            int randomYPos = Random.Range(posY - depthHeight, posY + depthHeight);
            if (randomXPos < 0 || randomYPos < 0)
                continue;
            if (m_map[randomXPos, randomYPos] >= 1 && m_map[randomXPos, randomYPos] != 100)
                continue;

            if (m_entityMap[randomXPos, randomYPos] == 1)
                continue;
            //free space
            bool surroundingClear = true;
            for (int x = randomXPos- entity.m_widthOfEntity; x < randomXPos + entity.m_widthOfEntity; x++)
            {
                for (int y = randomYPos- entity.m_lengthOfEntity; y < randomYPos + entity.m_lengthOfEntity; y++)
                {
                    if(y<0 || x<0 || x >= m_entityMap.GetLength(0) || y >= m_entityMap.GetLength(1))
                        continue;
                    if (m_entityMap[x, y] == 1)
                    {
                        surroundingClear = false;
                        break;
                    }
                }
            }
            if (surroundingClear)
            {
                m_entityMap[randomXPos, randomYPos] = 1;
                return new Vector2(randomXPos, randomYPos);
            }
            
        }
        return new Vector2(-1, -1);
    }

    public void ClearData()
    {
        m_map = null;
        m_entityMap = null;
        m_nodes=null;
        m_storedNodes = null;
        foreach (GameObject entity in m_entities)
        {
            Destroy(entity);
        }
        m_entities.Clear();
    }

}
