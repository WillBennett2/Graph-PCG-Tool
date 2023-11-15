using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Graph;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{ 
    [SerializeField] private List<Vector2NodeDataLinker> m_nodes;
    [SerializeField] private List<Vector2EdgeDataLinker> m_edges;

    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;
    void Start()
    {
        GraphInfo.m_graphInfo = new Graph(m_rows, m_columns);
        m_nodes = GraphInfo.m_graphInfo.m_nodes;
        m_edges = GraphInfo.m_graphInfo.m_edges;
    }


    void OnDrawGizmos()
    {
        if (m_nodes == null)
        {
            Start();
        }
        //drawing nodes
        foreach (var node in m_nodes)
        {
            Gizmos.color = node.m_nodeData.colour;
            Gizmos.DrawSphere(node.m_position, 0.125f);
        }
        //drawing edges
        foreach (var edge in m_edges)
        {
            Gizmos.color = edge.m_edgeData.colour;
            Gizmos.DrawLine(edge.m_edgeData.fromPos, edge.m_edgeData.toPos);
        }
    }

}

//[ExecuteInEditMode]
//public class GraphComponent : MonoBehaviour
//{

//    [Serializable]
//    public class Vector3FloatLiner
//    {
//        public Graph<Vector3,float> m_key = new Graph<Vector3, float>();
//        public Graph m_value;
//    }

//    [SerializeField] public List<Vector3FloatLiner> m_graph;
//    [SerializeField] public Graph<Vector3, float> graph;
//    // Start is called before the first frame update
//    void Start()
//    {
//        graph = new Graph<Vector3, float>();
//        var node1 = new Node<Vector3>() { Value = new Vector3(0, 0, 0), NodeColour = Color.red };
//        var node2 = new Node<Vector3>() { Value = new Vector3(1, 0, 0), NodeColour = Color.green };
//        var node3 = new Node<Vector3>() { Value = new Vector3(1, 0, -1), NodeColour = Color.green };
//        var node4 = new Node<Vector3>() { Value = new Vector3(0, 0, -1), NodeColour = Color.green };
//        var edge1 = new Edge<float, Vector3> { Value = 1.0f, From = node1, To = node2, EdgeColour = Color.white };



//        graph.Nodes.Add(node1);
//        graph.Nodes.Add(node2);
//        graph.Nodes.Add(node3);
//        graph.Nodes.Add(node4);

//        graph.Edges.Add(edge1);

//    }

//    private void CreateNode(Vector3 position, Color colour)
//    {
//        var node = new Node<Vector3>() { Value = position, NodeColour = colour };
//        graph.Nodes.Add(node);
//    }



//    void OnDrawGizmos()
//    {
//        if (graph == null)
//        {
//            Start();
//        }

//        //drawing nodes
//        foreach (var node in graph.Nodes)
//        {
//            Gizmos.color = node.NodeColour;
//            Gizmos.DrawSphere(node.Value, 0.125f);
//        }
//        //drawing edges
//        //foreach (var edge in graph.Edges)
//        //{
//        //    Gizmos.color = edge.EdgeColour;
//        //    Gizmos.DrawLine(edge.From.Value, edge.To.Value);
//        //}
//    }
//}



////[ExecuteInEditMode]
////public class GraphComponent : MonoBehaviour
////{
////    [Serializable]
////    public class Vector2IntLinker
////    {
////        public Vector2 key;
////        public int value;
////    }

////    [SerializeField] private List<Vector2IntLinker> exposedGraph;
////    [SerializeField]public Graph graph;
////    public Dictionary<Vector2, int> m_graph;

////    private void Start()
////    {
////        graph.m_graph = new Dictionary<Vector2, int>();
////        graph.m_graph.Add(new Vector2(0, 0), 1);
////    }
////}