using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Graph;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [SerializeField] public Rule m_ruleReference;
    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;
    [SerializeField] private char m_defaultSymbol = '/';

    [SerializeField] public List<Vector2NodeDataLinker> m_nodes=null;
    [SerializeField] private List<Vector2EdgeDataLinker> m_edges=null;

    [SerializeField] private GameObject m_nodePrefab;
    void Start()
    {
        GraphInfo.m_graphInfo = new Graph(m_rows, m_columns, m_defaultSymbol, GetComponent<Alphabet>());
        m_nodes = GraphInfo.m_graphInfo.m_nodes;
        m_edges = GraphInfo.m_graphInfo.m_edges;
    }
    private void InitGraph()
    {
        GraphInfo.m_graphInfo = new Graph(m_rows, m_columns, m_defaultSymbol,GetComponent<Alphabet>());
        m_nodes = GraphInfo.m_graphInfo.m_nodes;
        m_edges = GraphInfo.m_graphInfo.m_edges;
    }


    void OnDrawGizmos()
    {
        if (m_nodes == null)
        {
            InitGraph();
        }
        //drawing nodes
        foreach (var node in m_nodes)
        {
            Gizmos.color = node.m_nodeData.colour;
            Gizmos.DrawSphere(node.m_nodeData.position, 0.125f);
        }
        //drawing edges
        foreach (var edge in m_edges)
        {
            Gizmos.color = edge.m_edgeData.colour;
            Gizmos.DrawLine(edge.m_edgeData.fromPos, edge.m_edgeData.toPos);
        }
    }

}

