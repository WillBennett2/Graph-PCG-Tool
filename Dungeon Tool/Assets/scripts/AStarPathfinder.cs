using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Graph;

public class AStarPathfinder : MonoBehaviour
{
    private List<Index2NodeDataLinker> m_nodes;
    private int m_graphWidth;

    // A* Pathfinding Algorithm Pseudocode for Unity

    public void SetData(List<Index2NodeDataLinker> nodes, int width)
    {
        m_nodes = nodes;
        m_graphWidth = width;
    }

    // A* pathfinding function
    public List<Index2NodeDataLinker> FindPath(Index2NodeDataLinker startCell, Index2NodeDataLinker goalCell)
    {
        var openList = new List<Index2NodeDataLinker>(); // Cells to explore
        var closedList = new List<Index2NodeDataLinker>(); // Explored cells

        openList.Add(startCell);

        while (openList.Count > 0)
        {
            // Find the cell with the lowest FCost in the open list
            var currentCell = openList.OrderBy(cell => cell.nodeData.totalCost).First();

            // Remove the current cell from the open list
            openList.Remove(currentCell);
            closedList.Add(currentCell);

            // Check if we've reached the goal cell
            if (currentCell == goalCell)
                return ReconstructPath(currentCell);

            // Explore neighboring cells
            foreach (var neighbor in GetNeighbors(currentCell))
            {
                if (closedList.Contains(neighbor))
                    continue;

                var tentativeGCost = currentCell.nodeData.fromStartCost + Distance(currentCell, neighbor);

                if (!openList.Contains(neighbor) || tentativeGCost < neighbor.nodeData.fromStartCost)
                {
                    neighbor.nodeData.fromStartCost = tentativeGCost;
                    neighbor.nodeData.heuristicCost = Heuristic(neighbor, goalCell);
                    neighbor.nodeData.parent = currentCell;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // No path found
        return null;
    }

    // Helper functions
    List<Index2NodeDataLinker> ReconstructPath(Index2NodeDataLinker goalCell)
    {
        var path = new List<Index2NodeDataLinker>();
        var current = goalCell;

        while (current != null)
        {
            path.Add(current);
            current = current.nodeData.parent;
        }

        path.Reverse();
        return path;
    }

    List<Index2NodeDataLinker> GetNeighbors(Index2NodeDataLinker cell)
    {
        // Implement logic to get neighboring cells (up, down, left, right, and diagonals)
        // based on your grid representation.
        // Return a list of valid neighboring cells.
        List<Index2NodeDataLinker> validCells = new List<Index2NodeDataLinker>();

        //get up
        if(cell.nodeData.upperEdge.edgeData.terrian <=0)
            validCells.Add(m_nodes[cell.nodeData.upperEdge.edgeData.toNode]);
        //get right
        if (cell.nodeData.rightEdge.edgeData.terrian <= 0)
            validCells.Add(m_nodes[cell.nodeData.rightEdge.edgeData.toNode]);

        //get down
        if (0 < cell.index - 1)
        {
            if (m_nodes[cell.index - 1].nodeData.upperEdge.edgeData.terrian <= 0)
                validCells.Add(m_nodes[m_nodes[cell.index - 1].nodeData.upperEdge.edgeData.toNode]);
        }
        //get left
        if (0 < cell.index - m_graphWidth)
        {
            if (m_nodes[cell.index - m_graphWidth].nodeData.rightEdge.edgeData.terrian <= 0)
                validCells.Add(m_nodes[m_nodes[cell.index - m_graphWidth].nodeData.rightEdge.edgeData.toNode]);
        }

        return validCells;
    }

    float Distance(Index2NodeDataLinker currentNode, Index2NodeDataLinker toNode)
    {
        // Calculate the distance between two cells (e.g., Euclidean distance).
        // You can use Manhattan distance or other distance metrics as well.
        float distance = Math.Abs(currentNode.nodeData.position.x - toNode.nodeData.position.x) + Math.Abs(currentNode.nodeData.position.z-toNode.nodeData.position.z);
        return distance;
    }

    float Heuristic(Index2NodeDataLinker cell, Index2NodeDataLinker goalCell)
    {
        // Calculate the heuristic (estimated) cost from cell to goalCell.
        // You can use Euclidean distance, Manhattan distance, or other heuristics.
        float heuristic = Math.Abs(cell.nodeData.position.x - goalCell.nodeData.position.x ) + Math.Abs(cell.nodeData.position.z - goalCell.nodeData.position.z);
        return heuristic;
    }
}
