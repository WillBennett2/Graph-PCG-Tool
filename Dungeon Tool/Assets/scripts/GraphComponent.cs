using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using static Graph;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [SerializeField] public Rule m_ruleReference;
    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;
    [SerializeField] private string m_defaultSymbol = "unused";

    [SerializeField] public List<Index2NodeDataLinker> m_nodes = null;
    [SerializeField] public List<Index2EdgeDataLinker> m_edges = null;
    [SerializeField] public List<Index2StoredNodeDataLinker> m_storedNodes = null;

    [SerializeField] private GameObject m_nodePrefab;
    void Start()
    {
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_defaultSymbol, GetComponent<Alphabet>());
        m_nodes = GraphInfo.graphInfo.nodes;
        m_storedNodes = GraphInfo.graphInfo.storedNodes;
        m_edges = GraphInfo.graphInfo.edges;
    }
    private void InitGraph()
    {
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_defaultSymbol, GetComponent<Alphabet>());
        m_nodes = GraphInfo.graphInfo.nodes;
        m_storedNodes = GraphInfo.graphInfo.storedNodes;
        m_edges = GraphInfo.graphInfo.edges;
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

