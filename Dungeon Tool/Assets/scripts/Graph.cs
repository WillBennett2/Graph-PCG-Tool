using System;
using System.Collections.Generic;
using UnityEngine;
using static Alphabet;

[Serializable]
public class Graph
{
    [Serializable]
    public class Index2StoredNodeDataLinker
    {
        [SerializeField] public int index;
        [SerializeField] public StoredNodeData storedNodeData;
        public Index2StoredNodeDataLinker(int index, StoredNodeData storedNodeData)
        {
            this.index = index;
            this.storedNodeData = storedNodeData;
            this.storedNodeData.colour = storedNodeData.colour;      
            this.storedNodeData.position = storedNodeData.position;
        }
    }
    [Serializable]
    public struct StoredNodeData
    {
        public string symbol;
        [HideInInspector] public Vector3 position;
        [HideInInspector] public Color colour;
        public int parentIndex;
        public int terrain;
        public int item;
        public int enemy;
    }
    [Serializable]
    public struct NodeData
    {
        public string symbol;
        [HideInInspector] public Vector3 position;
        public Vector2 gridCoordinates;
        [HideInInspector] public Color colour;
        public int terrain;
        public int item;
        public int enemy;
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
        }
    }

    [Serializable]
    public struct EdgeData
    {
        public string symbol;
        public Vector3 position;
        [HideInInspector] public Color colour;
        [HideInInspector] public int graphFromNode;
        [HideInInspector] public int graphToNode;
        public int fromNode;
        public int toNode;
         public Vector3 fromPos;
         public Vector3 toPos;
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
    [SerializeField] public List<Index2StoredNodeDataLinker> storedNodes;
    [SerializeField] public List<Index2EdgeDataLinker> edges;

    private int m_graphSize;
    public int nodeIndexCounter;

    public Graph(int rows, int columns, string defaultSymbol, Alphabet alphabet)
    {
        nodeIndexCounter = m_graphSize = rows * columns;
        nodes = new List<Index2NodeDataLinker>();
        storedNodes = new List<Index2StoredNodeDataLinker>();
        edges = new List<Index2EdgeDataLinker>();
        int index = 0;
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                NodeData data = new NodeData();
                data.gridCoordinates = new Vector2 (x, y);
                data.position = new Vector3(x, y);
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
                    data.symbol = "edge";
                    data.colour = Color.white;
                    data.position = new Vector2(x, y + 0.1f);
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
                    data.symbol = "edge";
                    data.colour = Color.white;
                    data.position = new Vector3(x + 0.1f, y);
                    data.graphFromNode = edgeFromIndex;
                    data.graphToNode = edgeToIndex;
                    data.fromPos = data.position;
                    data.toPos = new Vector3(x + 1f, y);
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
            for (int j = 0; j <storedNodes.Count; j++)
            {
                if (storedNodes[j].storedNodeData.symbol == data.m_symbol)
                    storedNodes[j].storedNodeData.colour = data.m_colour;
            }
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
