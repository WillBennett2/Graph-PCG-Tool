using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Graph;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    public static event Action OnClearData;
    public static event Action<List<Index2NodeDataLinker>, List<Index2StoredNodeDataLinker>, bool, bool> OnSpawnEntities;
    public static event System.Action<List<Index2NodeDataLinker>, List<Index2EdgeDataLinker>, int, int, int, int> OnGenerateEnvrionment;
    public static event Action<List<Index2NodeDataLinker>, List<Index2StoredNodeDataLinker>, List<Index2EdgeDataLinker>> OnRunGraphGrammar;
    public static event Action<List<Index2NodeDataLinker>, Index2NodeDataLinker, Index2NodeDataLinker, int> OnFindValidPaths;

    public static event Action<List<Index2NodeDataLinker>,AnimationCurve,int> OnApplyDifficultyCurve;


    [Header("Graph Values")]
    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;
    [SerializeField] private string m_defaultSymbol = "unused";
    [SerializeField] private int m_scale = 1;
    [SerializeField] private int m_offset = 1;

    [Header("Graph Data")]
    public Alphabet m_alphabet;
    public List<Index2NodeDataLinker> m_nodes = null;
    public List<Index2EdgeDataLinker> m_edges = null;
    public List<Index2StoredNodeDataLinker> m_storedNodes = null;
    public List<Index2NodeDataLinker> m_pathList;
    private bool m_ruleApplied;

    [Header("Difficulty Curve")]
    [SerializeField] bool m_applyCurve = true;
    [Tooltip("X axis should be 0 to 1")]
    [SerializeField] private AnimationCurve m_difficultyCurve;
    [Tooltip("Value represents range applied to difficulty applied")]
    [Min(0)]
    [SerializeField] private int m_difficultyInterval;

    [Header("Entity Spawn Data")]
    [SerializeField]bool m_usePoisson;
    [SerializeField]bool m_useJitter;

    void Awake()
    {
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_scale, m_offset, m_defaultSymbol, m_alphabet);
        m_nodes = GraphInfo.graphInfo.nodes;
        m_storedNodes = GraphInfo.graphInfo.storedNodes;
        m_edges = GraphInfo.graphInfo.edges;

    }
    private void InitGraph()
    {
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_scale, m_offset, m_defaultSymbol, m_alphabet);
        m_nodes = GraphInfo.graphInfo.nodes;
        m_storedNodes = GraphInfo.graphInfo.storedNodes;
        m_edges = GraphInfo.graphInfo.edges;
    }

    public bool Generate()
    {
        InitGraph();
        OnRunGraphGrammar?.Invoke(m_nodes, m_storedNodes, m_edges);
        if (m_ruleApplied)
        {
            OnGenerateEnvrionment?.Invoke(m_nodes, m_edges, m_rows, m_columns, m_offset, m_scale);
            m_usePoisson = (m_useJitter == true ? false : true);
            m_useJitter = (m_usePoisson == true ? false : true);
            OnSpawnEntities?.Invoke(m_nodes, m_storedNodes, m_usePoisson, m_useJitter);
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

            OnFindValidPaths?.Invoke(m_nodes, endNode, startNode, m_rows);
            m_pathList.Add(startNode);
            m_pathList.Reverse();
            if(m_applyCurve)
                OnApplyDifficultyCurve?.Invoke(m_pathList,m_difficultyCurve,m_difficultyInterval);

            return true;
        }
        return false;

    }

    public void Reset()
    {
        OnClearData?.Invoke();
        //clear graph data
        m_nodes.Clear();
        m_storedNodes.Clear();
        m_edges.Clear();
        //clear tilemap
        //clear cave
        //clear entites done
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
            Handles.Label(node.nodeData.position, node.nodeData.difficultyRating.ToString());
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
                    direction = new Vector3(0, 0, -0.8f);
                    positionModifier = new Vector3(0, 0, 0.8f);
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
                    direction = m_nodes[edge.edgeData.toNode].nodeData.position - m_storedNodes[edge.edgeData.fromNode - (m_rows * m_columns) - 1].storedNodeData.position; // new Vector3(0, 0, -0.8f);
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

    public void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20)
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
            output += ("(");
            output += ("x=" + node.nodeData.position.x + ", ");
            output += ("y=" + node.nodeData.position.y + ", ");
            output += ("tileX=" + node.nodeData.position.x + ", ");
            output += ("tileY=" + node.nodeData.position.y);
            output += (")");
        }
        foreach (Index2EdgeDataLinker edge in m_edges)
        {
            output += (" ");
            output += (edge.edgeData.symbol);
            output += ("(");
            output += (edge.edgeData.fromNode + ", ");
            output += (edge.edgeData.toNode);
            if (!edge.edgeData.directional)
                output += (", d=" + edge.edgeData.directional.ToString().ToLower());
            output += (")");
        }
        foreach (Index2StoredNodeDataLinker node in m_storedNodes)
        {
            output += (" ");
            output += (node.storedNodeData.symbol);
            output += (" contain");
            output += ("(");
            output += (node.index + ", ");
            output += (node.storedNodeData.parentIndex + ", ");
            output += ("c=true");
            output += (")");
        }
        Debug.Log(output);
    }

    private void RuleApplied(bool ruleApplied)
    {
        m_ruleApplied = ruleApplied;
    }
    private void PathFound(List<Index2NodeDataLinker> pathList)
    {
        m_pathList = pathList;
    }

    private void OnEnable()
    {
        Rule.OnRuleApplied += RuleApplied;
        PathFinder.OnValidPathList += PathFound;
    }
    private void OnDisable()
    {
        Rule.OnRuleApplied -= RuleApplied;
        PathFinder.OnValidPathList -= PathFound;
    }

}

