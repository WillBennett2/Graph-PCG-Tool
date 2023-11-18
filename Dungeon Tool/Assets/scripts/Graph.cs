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
        public Vector2 position;
        public Color colour;
        public char symbol;
        public int terrain;
        public int item;
        public int enemy;
    }
    [Serializable] public class Vector2NodeDataLinker
    {
        [SerializeField] public int m_index;
        [SerializeField] public NodeData m_nodeData;

        public Vector2NodeDataLinker(int index,NodeData nodedata)
        {
            this.m_index = index;
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
    public int m_graphSize;
    //[SerializeField] public Dictionary<Vector2,NodeData> m_graph;
    public Graph(int rows, int columns, char defaultSymbol)
    {
        m_graphSize = rows*columns;
        m_nodes = new List<Vector2NodeDataLinker>();
        m_edges = new List<Vector2EdgeDataLinker>();
        int index = 0;
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                //Vector2 position = new Vector2(x, y);
                NodeData data = new NodeData();
                //m_graph.Add(position, data);
                data.position = new Vector2(x, y);
                data.symbol = defaultSymbol;
                var node = new Vector2NodeDataLinker(index,  data);
                m_nodes.Add(node);
                index++;
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
                if (x != columns - 1)
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
