using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.XR;
using static Graph;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [SerializeField]private CaveGenerator m_caveGenerator;
    [SerializeField] public Rule m_ruleReference;
    [SerializeField] private TileMap m_tileMap;
    [SerializeField] private EntitySpawner m_entitySpawner;
    [SerializeField] private PathFinder m_pathFinder;
    [SerializeField] private DifficultyCurve m_difficultyCurve;

    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;
    [SerializeField] private string m_defaultSymbol = "unused";
    [SerializeField] private int m_scale = 1;
    [SerializeField] private int m_offset = 1;

    [SerializeField] public List<Index2NodeDataLinker> m_nodes = null;
    [SerializeField] public List<Index2EdgeDataLinker> m_edges = null;
    [SerializeField] public List<Index2StoredNodeDataLinker> m_storedNodes = null;
    [SerializeField] public Alphabet m_alphabet;
    [SerializeField] public List<Index2NodeDataLinker> m_pathList;
    [SerializeField] private GameObject m_nodePrefab;
    public bool m_usePoisson;
    public bool m_useJitter;
    void Awake ()
    {
        ScaleSetup();
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_scale, m_offset, m_defaultSymbol, m_alphabet);
        m_nodes = GraphInfo.graphInfo.nodes;
        m_storedNodes = GraphInfo.graphInfo.storedNodes;
        m_edges = GraphInfo.graphInfo.edges;

    }
    private void InitGraph()
    {
        ScaleSetup();
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_scale, m_offset, m_defaultSymbol, m_alphabet);
        m_nodes = GraphInfo.graphInfo.nodes;
        m_storedNodes = GraphInfo.graphInfo.storedNodes;
        m_edges = GraphInfo.graphInfo.edges;
        SetUpData();
    }

    private void ScaleSetup()
    {
        m_caveGenerator.SetUpFromGraph(m_nodes, m_edges, m_rows, m_columns, m_offset, m_scale, m_entitySpawner, m_tileMap);
    }

    private void SetUpData()
    {
        m_entitySpawner.SetGraphData(m_nodes,m_storedNodes);
        m_caveGenerator.SetUpFromGraph(m_nodes, m_edges, m_rows, m_columns, m_offset, m_scale, m_entitySpawner, m_tileMap);
    }

    public bool Generate()
    {
        InitGraph();
        bool ruleApplied = m_ruleReference.RunRule(m_nodes, m_storedNodes, m_edges);
        if (ruleApplied)
        {
            ScaleSetup();
            m_caveGenerator.GenerateCave();
            m_usePoisson = (m_useJitter == true ? false : true);
            m_useJitter = (m_usePoisson == true ? false : true);
            m_entitySpawner.m_usePoisson =m_usePoisson;
            m_entitySpawner.m_useJitter = m_useJitter;
            m_entitySpawner.CreateEntity();
            Index2NodeDataLinker startNode = null;
            Index2NodeDataLinker endNode = null;
            foreach (var node in m_nodes)
            {
                if (node.nodeData.symbol == "Start")
                {
                    startNode = node;
                }
                if (node.nodeData.symbol == "End")
                {
                    endNode = node;
                }
            }

            m_pathList = m_pathFinder.RunSearch(m_nodes, endNode, startNode, m_rows);
            m_pathList.Add(startNode);
            m_pathList.Reverse();
            m_difficultyCurve.ApplyCurve(m_pathList);

            return true;
        }
        return false;

    }

    public void Reset()
    {
        //clear graph data
        m_nodes.Clear();
        m_storedNodes.Clear();
        m_edges.Clear();
        //clear tilemap
        m_tileMap.Clear();
        //clear cave
        m_caveGenerator.Clear();
        //clear entites
        m_entitySpawner.ClearData();
    }

    void OnDrawGizmos()
    {
        if (m_nodes == null)
        {
            InitGraph();
        }
        int rootOfGraph = (int)Mathf.Sqrt(m_rows * m_columns);
        //drawing nodes
        foreach (Index2NodeDataLinker node in m_nodes)
        {
            Gizmos.color = node.nodeData.colour;
            Gizmos.DrawSphere(node.nodeData.position, 0.125f);
        }
        //drawing contained nodes
        foreach (Index2StoredNodeDataLinker storednode in m_storedNodes)
        {
            Gizmos.color = storednode.storedNodeData.colour;
            Gizmos.DrawSphere(new Vector3(storednode.storedNodeData.position.x, 1f, storednode.storedNodeData.position.z), 0.125f);
        }


        //drawing edges
        foreach (var edge in m_edges)
        {
            if (edge.edgeData.directional)
            {
                int offset = edge.edgeData.toNode - edge.edgeData.fromNode;
                Vector3 direction = new Vector3();
                Vector3 positionModifier = new Vector3(0, 0, 0);
                if (offset == 1)
                {
                    //up
                    direction = new Vector3(0, 0, 0.8f);
                }
                else if (offset == -1)
                {
                    //down
                    direction = new Vector3(0,0, -0.8f);
                    positionModifier = new Vector3(0,  0,0.8f);
                }
                else if (offset == +rootOfGraph)
                {
                    //right
                    direction = new Vector3(0.8f, 0, 0);
                }
                else if (offset == -rootOfGraph)
                {
                    //left
                    direction = new Vector3(-0.8f, 0, 0);
                    positionModifier = new Vector3(0.8f, 0, 0);
                }
                else
                {
                    if (m_storedNodes.Count == 0)
                        break;
                    direction = m_nodes[edge.edgeData.toNode].nodeData.position - m_storedNodes[edge.edgeData.fromNode - (m_rows*m_columns) - 1].storedNodeData.position; // new Vector3(0, 0, -0.8f);
                    positionModifier = new Vector3(0, 0, 0);
                }
                DrawArrow(new Vector3(edge.edgeData.position.x, edge.edgeData.position.y, edge.edgeData.position.z) + positionModifier,
                    new Vector3(direction.x, direction.y, direction.z), edge.edgeData.colour);
            }
            else
            {
                Gizmos.color = edge.edgeData.colour;
                Gizmos.DrawLine(edge.edgeData.fromPos, edge.edgeData.toPos);
            }
        }
        //check if directional and then add a second small diag line
    }

    public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }


    public void PrintGraph()
    {
        string output = "";
        output += ("GRAPH");
        foreach (Index2NodeDataLinker node in m_nodes)
        {

            output += (" ");
            output += (node.nodeData.symbol);
            output +=("(");
            output +=("x="+node.nodeData.position.x + ", ");
            output +=("y="+node.nodeData.position.y + ", ");
            output +=("tileX=" + node.nodeData.position.x + ", ");
            output +=("tileY=" + node.nodeData.position.y);
            output +=(")");
        }
        foreach (Index2EdgeDataLinker edge in m_edges)
        {
            output +=(" ");
            output +=(edge.edgeData.symbol);
            output +=("(");
            output +=(edge.edgeData.fromNode + ", ");
            output += (edge.edgeData.toNode);
            if(!edge.edgeData.directional)
                output += (", d=" + edge.edgeData.directional.ToString().ToLower());
            output += (")");
        }
        foreach (Index2StoredNodeDataLinker node in m_storedNodes)
        {
            output +=(" ");
            output +=(node.storedNodeData.symbol);
            output +=(" contain");
            output +=("(");
            output +=(node.index + ", ");
            output +=(node.storedNodeData.parentIndex + ", ");
            output +=("c=true");
            output +=(")");
        }
        Debug.Log(output);
    }

}

