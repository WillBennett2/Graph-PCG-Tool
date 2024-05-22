using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using static Graph;

[ExecuteInEditMode]
public class PathFinder
{
    public static event Action<List<Index2NodeDataLinker>> OnValidPathList;

    int m_graphWidth;
    List<Index2NodeDataLinker> m_pathList;

    private void OnEnable()
    {
        GraphComponent.OnClearData += Clear;
        GraphComponent.OnDisableScripts += OnDisable;
        GraphComponent.OnFindValidPaths += RunSearch;
    }
    private void OnDisable()
    {
        GraphComponent.OnClearData -= Clear;
        GraphComponent.OnDisableScripts -= OnDisable;
        GraphComponent.OnFindValidPaths -= RunSearch;
    }
    public PathFinder()
    {
        OnEnable();
    }
    void Bfs(List<Index2NodeDataLinker> nodes, Index2NodeDataLinker endVertex, Index2NodeDataLinker startVertex)
    {
        bool[] visited = new bool[nodes.Count];
        Queue<Index2NodeDataLinker> queue = new Queue<Index2NodeDataLinker>();
        queue.Enqueue(endVertex);

        while (queue.Count > 0)
        {
            Index2NodeDataLinker currentVertex = queue.Dequeue();
            if (visited[currentVertex.index]) continue;
            else visited[currentVertex.index] = true;
            m_pathList.Add(currentVertex);
            List<Index2NodeDataLinker> neighbors = GetNeighbors(nodes, currentVertex);
            if (neighbors == null) continue;

            foreach (var neighbor in neighbors)
            {
                if (neighbor.nodeData.symbol == startVertex.nodeData.symbol)
                    continue;
                queue.Enqueue(neighbor);
            }
        }
    }
    List<Index2NodeDataLinker> GetNeighbors(List<Index2NodeDataLinker> nodes, Index2NodeDataLinker cell)
    {
        List<Index2NodeDataLinker> validCells = new List<Index2NodeDataLinker>();

        //get up
        if (cell.nodeData.upperEdge.edgeData.terrian <= 0)
            validCells.Add(nodes[cell.nodeData.upperEdge.edgeData.graphToNode]);
        //get right
        if (cell.nodeData.rightEdge.edgeData.terrian <= 0)
            validCells.Add(nodes[cell.nodeData.rightEdge.edgeData.graphToNode]);

        //get down
        if (0 < cell.index - 1)
        {
            if (nodes[cell.index - 1].nodeData.upperEdge.edgeData.terrian <= 0)
                validCells.Add(nodes[nodes[cell.index - 1].nodeData.upperEdge.edgeData.graphFromNode]);
        }
        //get left
        if (0 < cell.index - m_graphWidth)
        {
            if (nodes[cell.index - m_graphWidth].nodeData.rightEdge.edgeData.terrian <= 0)
                validCells.Add(nodes[nodes[cell.index - m_graphWidth].nodeData.rightEdge.edgeData.graphFromNode]);
        }
        return validCells;
    }
    public void RunSearch(List<Index2NodeDataLinker> nodes, Index2NodeDataLinker endNode, Index2NodeDataLinker startNode, int graphWidth)
    {
        m_pathList = new List<Index2NodeDataLinker>();
        m_graphWidth = graphWidth;
        Bfs(nodes, endNode, startNode);
        OnValidPathList?.Invoke(m_pathList);
    }
    public void Clear()
    {
        if(m_pathList!=null)
            m_pathList.Clear();
    }
}

