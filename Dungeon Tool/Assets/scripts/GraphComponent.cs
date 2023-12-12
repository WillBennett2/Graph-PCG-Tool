using System.Collections.Generic;
using UnityEngine;
using static Graph;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [SerializeField] public Rule m_ruleReference;
    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;
    [SerializeField] private char m_defaultSymbol = '/';

    [SerializeField] public List<Index2NodeDataLinker> m_nodes = null;
    [SerializeField] public List<Index2EdgeDataLinker> m_edges = null;

    [SerializeField] private GameObject m_nodePrefab;
    void Start()
    {
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_defaultSymbol, GetComponent<Alphabet>());
        m_nodes = GraphInfo.graphInfo.nodes;
        m_edges = GraphInfo.graphInfo.edges;
    }
    private void InitGraph()
    {
        GraphInfo.graphInfo = new Graph(m_rows, m_columns, m_defaultSymbol, GetComponent<Alphabet>());
        m_nodes = GraphInfo.graphInfo.nodes;
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
        foreach (var node in m_nodes)
        {
            Gizmos.color = node.nodeData.colour;
            Gizmos.DrawSphere(node.nodeData.position, 0.125f);
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
                    direction = new Vector3(0, 0.8f, 0);
                }
                else if (offset == -1)
                {
                    //down
                    direction = new Vector3(0, -0.8f, 0);
                    positionModifier = new Vector3(0, 0.8f, 0);
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
                DrawArrow(new Vector3(edge.edgeData.position.x, edge.edgeData.position.y) + positionModifier, new Vector3(direction.x, direction.y, 0), edge.edgeData.colour);
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

}

