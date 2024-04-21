using System;
using System.Collections.Generic;
using System.Data;
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
        public EntitySpawnSetSO entitySet;
    }
    [Serializable]
    public struct NodeData
    {
        public string symbol;
        //[HideInInspector] 
        public Vector3 position;
        public Vector2 gridCoordinates;
        [HideInInspector] public Color colour;
        public int terrain;
        public bool preAuthored;
        public Index2EdgeDataLinker upperEdge;
        public Index2EdgeDataLinker rightEdge;
        public int spaceWidth;
        public int spaceHeight;
        public int difficultyRating;
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
            if(nodedata.terrain<0 || 1< nodedata.terrain)
            {
                this.nodeData.terrain = nodedata.terrain;
            }
            else
                this.nodeData.terrain = 1; //default of walls
            this.nodeData.preAuthored = false;
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
        [HideInInspector] public Quaternion rotation;
        public int fromNode;
        public int toNode;
        public Vector3 fromPos;
        public Vector3 toPos;
        public bool directional;
        public int terrian;
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
            this.edgeData.terrian = 1;
        }
    }
    public Alphabet m_alphabet;
    [HideInInspector] public int m_scale;
    private int m_offset = 1;
    public List<Index2NodeDataLinker> nodes;
    public List<Index2StoredNodeDataLinker> storedNodes;
    public List<Index2EdgeDataLinker> edges;

    private int m_graphSize;
    public int nodeIndexCounter;

    public Graph(int rows, int columns, int scale,int offset, string defaultSymbol, Alphabet alphabet)
    {
        m_alphabet = alphabet;
        m_scale = scale;
        m_offset = offset;
        nodeIndexCounter = m_graphSize = rows * columns;
        nodes = new List<Index2NodeDataLinker>();
        storedNodes = new List<Index2StoredNodeDataLinker>();
        edges = new List<Index2EdgeDataLinker>();
        int index = 0;
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                NodeData data = new NodeData();
                data.gridCoordinates = new Vector2 (x,z);
                data.position = new Vector3((x+ m_offset) * scale, 0,(z + m_offset) * scale);
                data.symbol = defaultSymbol;
                var node = new Index2NodeDataLinker(index, data);
                nodes.Add(node);
                index++;
            }
        }

        int nodeIndex = 0;
        index = 0;
        int edgeFromIndex = 0;
        int edgeToIndex = (int)MathF.Sqrt(rows * columns);
        for (float x = 0; x < rows; x++)
        {
            for (float z = 0; z < columns; z++)
            {
                EdgeData data = new EdgeData();
                data.symbol = "edge";
                data.colour = Color.white;
                data.position = new Vector3((x + m_offset) * scale, 0, (z + m_offset) * scale);
                data.graphFromNode = edgeFromIndex;
                data.graphToNode = edgeFromIndex + 1;
                data.fromPos = data.position;
                data.toPos = new Vector3((x + m_offset) * scale, 0, (z + m_offset + 1f) * scale);
                data.rotation = SetNormalRotation(data);
                var edge = new Index2EdgeDataLinker(index, data);

                if (z != rows - 1) //up
                {
                    nodes[nodeIndex].nodeData.upperEdge = edge;
                    edges.Add(edge);
                    index++;
                }
                else
                {
                    var edgeNull = new Index2EdgeDataLinker(-1, data);
                    nodes[nodeIndex].nodeData.upperEdge = edgeNull;
                }

                data = new EdgeData();
                data.symbol = "edge";
                data.colour = Color.white;
                data.position = new Vector3((x + m_offset) * scale, 0, (z + m_offset) * scale);
                data.graphFromNode = edgeFromIndex;
                data.graphToNode = edgeToIndex;
                data.fromPos = data.position;
                data.toPos = new Vector3((x + m_offset + 1f) * scale, 0, (z + m_offset) * scale);
                data.rotation = SetNormalRotation(data);
                edge = new Index2EdgeDataLinker(index, data);

                if (x != columns - 1) //right
                {
                    nodes[nodeIndex].nodeData.rightEdge = edge;
                    edges.Add(edge);
                    index++;
                }
                else
                {
                    var edgeNull = new Index2EdgeDataLinker(-1, data);
                    nodes[nodeIndex].nodeData.rightEdge = edgeNull;
                }

                edgeFromIndex++;
                edgeToIndex++;
                nodeIndex++;
            }
        }

        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
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

    public Quaternion SetRotation(EdgeData data)
    {
        //figure out direction needed
        if(data.toNode - data.fromNode == 1)
        {
            //set data rotation to upward
            data.rotation = Quaternion.Euler(90, 0, 0);
        }
        else if(data.toNode - data.fromNode == -1)
        {
            //set data rotation to downward
            data.rotation = Quaternion.Euler(-90, 0, 0);
        }
        else if(data.toNode - data.fromNode == Mathf.Round( Mathf.Sqrt(m_graphSize)))
        {
            //right
            data.rotation = Quaternion.Euler(90, 0, -90);
        }
        else if(data.toNode - data.fromNode == -Mathf.Round(Mathf.Sqrt(m_graphSize)))
        {
            //left
            data.rotation = Quaternion.Euler(90, 0, 90);
        }

        return data.rotation;
    }
    public Quaternion SetNormalRotation(EdgeData data)
    {
        //figure out direction needed
        if (data.graphToNode - data.graphFromNode == 1)
        {
            //set data rotation to upward
            data.rotation = Quaternion.Euler(90, 0, 0);
        }
        else if (data.graphToNode - data.graphFromNode == -1)
        {
            //set data rotation to downward
            data.rotation = Quaternion.Euler(-90, 0, 0);
        }
        else if (data.graphToNode - data.graphFromNode == Mathf.Round(Mathf.Sqrt(m_graphSize)))
        {
            //right
            data.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (data.graphToNode - data.graphFromNode == -Mathf.Round(Mathf.Sqrt(m_graphSize)))
        {
            //left
            data.rotation = Quaternion.Euler(0, 0, -90);
        }

        return data.rotation;
    }


}
[Serializable]
public static class GraphInfo
{
    public static Graph graphInfo;
}
