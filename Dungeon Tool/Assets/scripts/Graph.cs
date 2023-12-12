using System;
using System.Collections.Generic;
using UnityEngine;
using static Alphabet;

[Serializable]
public class Graph
{
    [Serializable]
    public struct StoredNodeData
    {
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Color colour;
        public int index;
        public char symbol;
        public int parentIndex;
        public int terrain;
        public int item;
        public int enemy;

        public void SetIndex(int graphIndex)
        {
            index = graphIndex;
        }
        public void SetParentIndex(int index)
        {
            parentIndex = index;
        }
    }
    [Serializable]
    public struct NodeData
    {
        public Vector2 position;
        [HideInInspector] public Color colour;
        public char symbol;
        public int terrain;
        public int item;
        public int enemy;
        public List<StoredNodeData> storedNodes;
    }
    [Serializable]
    public class Index2NodeDataLinker
    {
        [SerializeField] public int m_index;
        [SerializeField] public NodeData m_nodeData;

        public Index2NodeDataLinker(int index, NodeData nodedata)
        {
            this.m_index = index;
            this.m_nodeData = nodedata;
            this.m_nodeData.colour = nodedata.colour;
            this.m_nodeData.storedNodes = new List<StoredNodeData>();
        }
    }

    [Serializable]
    public struct EdgeData
    {
        public char symbol;
        public Vector2 position;
        [HideInInspector]public Color colour;
        public int fromNode;
        public int toNode;
        [HideInInspector] public int directionalFromNode;
        [HideInInspector] public int directionalToNode;
        public Vector2 fromPos;
        public Vector2 toPos;
        public bool directional;
    }
    [Serializable]
    public class Index2EdgeDataLinker
    {
        [SerializeField] public int m_index;
        [SerializeField] public EdgeData m_edgeData;

        public Index2EdgeDataLinker(int index, EdgeData edgeData)
        {
            this.m_index = index;
            this.m_edgeData = edgeData;
            this.m_edgeData.colour = edgeData.colour;
            this.m_edgeData.directionalFromNode = m_edgeData.fromNode;
            this.m_edgeData.directionalToNode = m_edgeData.toNode;
        }
    }

    [SerializeField] public List<Index2NodeDataLinker> m_nodes;
    [SerializeField] public List<Index2EdgeDataLinker> m_edges;

    private int m_graphSize;
    public int m_nodeIndexCounter;
    //[SerializeField] public Dictionary<Vector2,NodeData> m_graph;
    public Graph(int rows, int columns, char defaultSymbol, Alphabet alphabet)
    {
        m_nodeIndexCounter = m_graphSize = rows * columns;
        m_nodes = new List<Index2NodeDataLinker>();
        m_edges = new List<Index2EdgeDataLinker>();
        int index = 0;
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                NodeData data = new NodeData();
                data.position = new Vector2(x, y);
                data.symbol = defaultSymbol;
                var node = new Index2NodeDataLinker(index, data);
                m_nodes.Add(node);
                index++;
            }
        }

        index = 0;
        int edgeFromIndex = 0;
        int edgeToIndex = (int)MathF.Sqrt(rows * columns);
        for (float x = 0; x < rows; x++)
        {
            for (float y = 0; y < columns; y++)
            {
                if (y != rows - 1) //up
                {
                    EdgeData data = new EdgeData();
                    data.symbol = defaultSymbol;
                    data.colour = Color.white;
                    data.position = new Vector2(x , y + 0.1f);
                    data.fromNode = edgeFromIndex;
                    data.toNode = edgeFromIndex + 1;
                    data.fromPos = data.position;
                    data.toPos = new Vector2(x, y + 1f);
                    var edge = new Index2EdgeDataLinker(index, data);
                    m_edges.Add(edge);
                    index++;
                }
                if (x != columns - 1) //right
                {
                    EdgeData data = new EdgeData();
                    data.symbol = defaultSymbol;
                    data.colour = Color.white;
                    data.position = new Vector2(x + 0.1f, y);
                    data.fromNode = edgeFromIndex;
                    data.toNode = edgeToIndex;
                    data.fromPos = data.position;
                    data.toPos = new Vector2(x+1f, y);
                    var edge = new Index2EdgeDataLinker(index, data);
                    m_edges.Add(edge);
                    index++;
                }
                edgeFromIndex++;
                edgeToIndex++;
            }
        }

        foreach (AlphabetLinker data in alphabet.m_alphabet)
        {
            for (int i = 0; i < m_nodes.Count; i++)
            {
                if (m_nodes[i].m_nodeData.symbol == data.m_symbol)
                    m_nodes[i].m_nodeData.colour = data.m_colour;
            }
        }
        foreach (AlphabetLinker data in alphabet.m_alphabet)
        {
            for (int i = 0; i < m_edges.Count; i++)
            {
                if (m_edges[i].m_edgeData.symbol == data.m_symbol)
                    m_edges[i].m_edgeData.colour = data.m_colour;
            }
        }

    }

}
[Serializable]
public static class GraphInfo
{
    public static Graph m_graphInfo;
}
