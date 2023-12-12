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
        [SerializeField] public int index;
        [SerializeField] public NodeData nodeData;

        public Index2NodeDataLinker(int index, NodeData nodedata)
        {
            this.index = index;
            this.nodeData = nodedata;
            this.nodeData.colour = nodedata.colour;
            this.nodeData.storedNodes = new List<StoredNodeData>();
        }
    }

    [Serializable]
    public struct EdgeData
    {
        public char symbol;
        [HideInInspector] public Vector2 position;
        [HideInInspector]public Color colour;
        [HideInInspector] public int graphFromNode;
        [HideInInspector] public int graphToNode;
        public int fromNode;
        public int toNode;
        [HideInInspector] public Vector2 fromPos;
        [HideInInspector] public Vector2 toPos;
        public bool directional;
    }
    [Serializable]
    public class Index2EdgeDataLinker
    {
        [SerializeField] public int index;
        [SerializeField] public EdgeData edgeData;

        public Index2EdgeDataLinker(int index, EdgeData edgeData)
        {
            this.index = index;
            this.edgeData = edgeData;
            this.edgeData.colour = edgeData.colour;
            this.edgeData.fromNode = this.edgeData.graphFromNode;
            this.edgeData.toNode = this.edgeData.graphToNode;
        }
    }

    [SerializeField] public List<Index2NodeDataLinker> nodes;
    [SerializeField] public List<Index2EdgeDataLinker> edges;

    private int m_graphSize;
    public int nodeIndexCounter;

    public Graph(int rows, int columns, char defaultSymbol, Alphabet alphabet)
    {
        nodeIndexCounter = m_graphSize = rows * columns;
        nodes = new List<Index2NodeDataLinker>();
        edges = new List<Index2EdgeDataLinker>();
        int index = 0;
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                NodeData data = new NodeData();
                data.position = new Vector2(x, y);
                data.symbol = defaultSymbol;
                var node = new Index2NodeDataLinker(index, data);
                nodes.Add(node);
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
                    data.graphFromNode = edgeFromIndex;
                    data.graphToNode = edgeFromIndex + 1;
                    data.fromPos = data.position;
                    data.toPos = new Vector2(x, y + 1f);
                    var edge = new Index2EdgeDataLinker(index, data);
                    edges.Add(edge);
                    index++;
                }
                if (x != columns - 1) //right
                {
                    EdgeData data = new EdgeData();
                    data.symbol = defaultSymbol;
                    data.colour = Color.white;
                    data.position = new Vector2(x + 0.1f, y);
                    data.graphFromNode = edgeFromIndex;
                    data.graphToNode = edgeToIndex;
                    data.fromPos = data.position;
                    data.toPos = new Vector2(x+1f, y);
                    var edge = new Index2EdgeDataLinker(index, data);
                    edges.Add(edge);
                    index++;
                }
                edgeFromIndex++;
                edgeToIndex++;
            }
        }

        foreach (AlphabetLinker data in alphabet.m_alphabet)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].nodeData.symbol == data.m_symbol)
                    nodes[i].nodeData.colour = data.m_colour;
            }
        }
        foreach (AlphabetLinker data in alphabet.m_alphabet)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].edgeData.symbol == data.m_symbol)
                    edges[i].edgeData.colour = data.m_colour;
            }
        }

    }

}
[Serializable]
public static class GraphInfo
{
    public static Graph graphInfo;
}
