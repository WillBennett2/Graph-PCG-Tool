using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using static Graph;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PathFinder : MonoBehaviour
{
    int m_graphWidth;
    List<Index2NodeDataLinker> m_pathList;

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
                //Debug.Log(neighbor.nodeData.symbol);
                if (neighbor.nodeData.symbol == startVertex.nodeData.symbol)
                    continue;
                queue.Enqueue(neighbor);
            }
        }
    }
    List<Index2NodeDataLinker> GetNeighbors(List<Index2NodeDataLinker> nodes, Index2NodeDataLinker cell)
    {
        // Implement logic to get neighboring cells (up, down, left, right, and diagonals)
        // based on your grid representation.
        // Return a list of valid neighboring cells.
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
    public List<Index2NodeDataLinker> RunSearch(List<Index2NodeDataLinker> nodes, Index2NodeDataLinker endNode, Index2NodeDataLinker startNode, int graphWidth)
    {
        m_pathList = new List<Index2NodeDataLinker>();
        m_graphWidth = graphWidth;
        Bfs(nodes, endNode,startNode);

        return m_pathList;
    }
}
