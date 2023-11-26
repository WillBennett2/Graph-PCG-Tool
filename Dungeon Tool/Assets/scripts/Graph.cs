using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Alphabet;
using static Graph;

[Serializable]
public class Graph
{
    [Serializable] public struct NodeData
    {
        [HideInInspector]public Vector2 position;
        [HideInInspector]public Color colour;
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
            this.m_nodeData.colour = nodedata.colour;
        }
    }

    [Serializable]
    public struct EdgeData
    {
        public char symbol;
        public Vector2 position;
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
        [SerializeField] public int m_index;
        [SerializeField] public EdgeData m_edgeData;

        public Vector2EdgeDataLinker(int index, EdgeData edgeData)
        {
            this.m_index= index;
            this.m_edgeData = edgeData;
            this.m_edgeData.colour = edgeData.colour;
        }
    }

    [SerializeField] public List<Vector2NodeDataLinker> m_nodes;
    [SerializeField] public List<Vector2EdgeDataLinker> m_edges;
    public int m_graphSize;
    //[SerializeField] public Dictionary<Vector2,NodeData> m_graph;
    public Graph(int rows, int columns, char defaultSymbol, Alphabet alphabet)
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

        index = 0;
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if(y != rows-1)
                {
                    //Vector2 position = new Vector2(x, y);
                    EdgeData data = new EdgeData();
                    data.symbol = defaultSymbol;
                    data.position = new Vector2(x, y);
                    data.from = x + y;
                    data.to = (x + y) + 1;
                    data.fromPos = data.position;
                    data.toPos = new Vector2(x, y + 1);
                    var edge = new Vector2EdgeDataLinker(index, data);
                    m_edges.Add(edge);
                    index++;
                }
                if (x != columns - 1)
                {
                    //Vector2 position = new Vector2(x, y);
                    EdgeData data = new EdgeData();
                    data.symbol = defaultSymbol;
                    data.position = new Vector2(x, y);
                    data.from = x+y;
                    data.to = (x + y) + (int)MathF.Sqrt(rows*columns);
                    data.fromPos = data.position;
                    data.toPos = new Vector2(x + 1, y);
                    var edge = new Vector2EdgeDataLinker(index, data);
                    m_edges.Add(edge);
                    index++;
                }
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
