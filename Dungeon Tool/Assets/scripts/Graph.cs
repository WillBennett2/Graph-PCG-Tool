using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Graph;

[Serializable]
public class Graph
{
    [Serializable] public struct NodeData
    {
        public Color colour;
        public char symbol;
        public int terrain;
        public int item;
        public int enemy;
    }
    [Serializable] public class Vector2NodeDataLinker
    {
        [SerializeField] public Vector2 m_position;
        [SerializeField] public NodeData m_nodeData;

        public Vector2NodeDataLinker(Vector2 position,NodeData nodedata)
        {
            this.m_position = position;
            this.m_nodeData = nodedata;
            this.m_nodeData.colour = Color.white;
        }
    }

    [Serializable]
    public struct EdgeData
    {
        public Color colour;
        public int from;
        public int to;
        public Vector2 fromPos;
        public Vector2 toPos;
        public bool directional;
    }
    [Serializable]
    public class Vector2EdgeDataLinker
    {
        [SerializeField] public Vector2 m_position;
        [SerializeField] public EdgeData m_edgeData;

        public Vector2EdgeDataLinker(Vector2 position, EdgeData edgeData)
        {
            this.m_position = position;
            this.m_edgeData = edgeData;
            this.m_edgeData.colour = Color.white;
        }
    }

    [SerializeField] public List<Vector2NodeDataLinker> m_nodes;
    [SerializeField] public List<Vector2EdgeDataLinker> m_edges;
    //[SerializeField] public Dictionary<Vector2,NodeData> m_graph;
    public Graph(int rows, int columns)
    {
        m_nodes = new List<Vector2NodeDataLinker>();
        m_edges = new List<Vector2EdgeDataLinker>();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                Vector2 position = new Vector2(x, y);
                NodeData data = new NodeData();
                //m_graph.Add(position, data);
                var node = new Vector2NodeDataLinker(new Vector2(x, y),  data);
                m_nodes.Add(node);
            }
        }

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if(y != rows-1)
                {
                    Vector2 position = new Vector2(x, y);
                    EdgeData data = new EdgeData();
                    data.from = x;
                    data.to = y;
                    data.fromPos = position;
                    data.toPos = new Vector2(x, y + 1);
                    var edge = new Vector2EdgeDataLinker(new Vector2(x, y), data);
                    m_edges.Add(edge);
                }
            }
        }
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (x!= columns - 1)
                {
                    Vector2 position = new Vector2(x, y);
                    EdgeData data = new EdgeData();
                    data.from = x;
                    data.to = y;
                    data.fromPos = position;
                    data.toPos = new Vector2(x + 1, y);
                    var edge = new Vector2EdgeDataLinker(new Vector2(x, y), data);
                    m_edges.Add(edge);
                }
            }
        }

    }

}
[Serializable]
public static class GraphInfo
{
    public static Graph m_graphInfo;
}




//[Serializable]
//public class Graph<TNodeType, TEdgeType>
//{
//    public Graph()
//    {
//        Nodes = new List<Node<TNodeType>>();
//        Edges = new List<Edge<TEdgeType, TNodeType>>();
//    }
//    public List<Node<TNodeType>> Nodes { get; private set; }
//    public List<Edge<TEdgeType, TNodeType>> Edges { get; private set; }
//}
//[Serializable]
//public class Node<Vector2>
//{
//    public Color NodeColour { get; set; }
//    public Vector2 Value { get; set; }
//}
//[Serializable]
//public class Edge<TEdgeType, TNodeType>
//{
//    public Color EdgeColour { get; set; }
//    public TEdgeType Value { get; set; }
//    public Node<TNodeType> From { get; set; }
//    public Node<TNodeType> To { get; set; }
//}

