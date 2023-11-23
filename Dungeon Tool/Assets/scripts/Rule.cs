using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static Graph;

[ExecuteInEditMode]
public class Rule : MonoBehaviour
{
    //[SerializeField] public bool m_runOnce;
    //[HideInInspector] public int m_maxIterations { get; set; }
    //[SerializeField] private Vector2 m_nodePosition = new Vector2(-1, -1);
    //[SerializeField] private char m_symbol = ' ';
    ////[SerializeField] private NodeData m_newNodeData;
    //[SerializeField] private List<NodeData> m_nodeDataList;
    [SerializeField] private RuleScriptableObject m_rule;
    private Vector2 m_position;
    private List<Vector2NodeDataLinker> m_graph = new List<Vector2NodeDataLinker>();
    [SerializeField] private List<Vector2NodeDataLinker> m_matchingNodes = new List<Vector2NodeDataLinker>();
    [SerializeField] private List<Vector2NodeDataLinker> m_matchingDirectionNodes = new List<Vector2NodeDataLinker>();
    private int m_originFoundIndex;
    [SerializeField]private List<Vector2NodeDataLinker> m_nodesToChange = new List<Vector2NodeDataLinker>();

    public RuleScriptableObject m_ruleRef { get; private set; }

    private void Awake()
    {
        m_ruleRef = m_rule;
        m_position = m_rule.m_firstNodePos;
    }
    public void Replace(List<Vector2NodeDataLinker> nodes)
    {
        m_matchingNodes.Clear();
        Vector2NodeDataLinker matchingNode = null;
        m_graph = nodes;
        PopulateMatchingNodes(nodes);

        if(m_rule.m_runOnce)
        {
            for (int i = 0; i < m_rule.m_nodeDataList.Count; i++)
            {
                if (i >= 1 && 1<= m_rule.m_nodeDataList.Count)
                {
                    Debug.Log("loops done = " + i);
                    matchingNode = GetNeighbouringNodes();
                    if (matchingNode != null)
                    {
                        m_nodesToChange.Add(matchingNode);
                        matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                        matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                    }

                    //reset
                    m_matchingNodes.Clear();
                    m_position = new Vector2(-1, -1);
                    PopulateMatchingNodes(nodes);
                }
                else if (1 < m_rule.m_nodeDataList.Count)
                {
                    matchingNode = GetMatchingNodes();
                    m_nodesToChange.Add(matchingNode);
                    matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                    matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                }
            }
            ApplyChanges();
        }

        if (!m_rule.m_runOnce)
        {
            for (int j = 1; j <= m_rule.m_maxIterations; j++)
            {
                for (int i = 0; i < m_rule.m_nodeDataList.Count; i++)
                {
                    if (i >= 1 && m_rule.m_nodeDataList.Count >= 1)
                    {
                        Debug.Log("loops done = " + i);
                        matchingNode = GetNeighbouringNodes();
                        if (matchingNode != null)
                        {
                            m_nodesToChange.Add(matchingNode);
                            matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                            matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                        }

                        //reset
                        m_matchingNodes.Clear();
                        m_position = new Vector2(-1, -1);
                        PopulateMatchingNodes(nodes);
                    }
                    else if (m_rule.m_nodeDataList.Count > 1)
                    {

                        matchingNode = GetMatchingNodes();
                        m_nodesToChange.Add(matchingNode);
                        matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                        matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                    }
                }
                ApplyChanges();
            }
        }
    }
    private void PopulateMatchingNodes(List<Vector2NodeDataLinker> nodes)
    {
        foreach (Vector2NodeDataLinker node in nodes)
        {
            if (node.m_nodeData.symbol == m_rule.m_symbol && (m_position == new Vector2(-1, -1) || node.m_nodeData.position == m_position))
            {
                m_matchingNodes.Add(node);
            }
        }

    }
    private void PopulateMatchingDirectionNodes(List<Vector2NodeDataLinker> nodes, Vector2 direction)
    {
        foreach (Vector2NodeDataLinker node in nodes)
        {
            if (node.m_nodeData.position == direction)
            {
                m_matchingDirectionNodes.Add(node);
            }
        }
    }
    private Vector2NodeDataLinker GetNeighbouringNodes()
    {
        Vector2NodeDataLinker node = null;
        Vector2NodeDataLinker lastNode = m_graph[m_originFoundIndex]; //hint

        for (int x = -1; x < 2; x++)
        {
            Vector2 posToGetX = new Vector2(x, 0) + lastNode.m_nodeData.position;
            if (posToGetX != lastNode.m_nodeData.position)
                PopulateMatchingDirectionNodes(m_graph, posToGetX);
            Vector2 posToGetY = new Vector2(0, x) + lastNode.m_nodeData.position;
            if (posToGetY != lastNode.m_nodeData.position)
                PopulateMatchingDirectionNodes(m_graph, posToGetY);
        }

        if (0 < m_matchingDirectionNodes.Count)
        {
            Debug.Log("node replaced");
            node = GetValidFreeNode();
            if (node != null)
            {
                m_originFoundIndex = node.m_index;
                Debug.Log("origin index " + m_originFoundIndex);
            }
        }
        m_matchingDirectionNodes.Clear();
        return node;
    }
    private Vector2NodeDataLinker GetValidFreeNode()
    {
        bool valid = false;
        int count = 0;
        Vector2NodeDataLinker node = null;

        while (valid == false)
        {
            if (m_matchingDirectionNodes.Count == 0)//if all 4 directions aren't valid then it cant be done
            {
                valid = true;
                break;
            }
            int index = UnityEngine.Random.Range(0, m_matchingDirectionNodes.Count);
            if (m_matchingDirectionNodes[index].m_nodeData.symbol == m_rule.m_symbol)
            {
                valid = true;
                node = m_matchingDirectionNodes[index];
            }
            else // if not valid then remove
            {
                m_matchingDirectionNodes.RemoveAt(index);
            }
            count++;
        }

        return node;
    }
    private void ApplyChanges()
    {
        if (m_nodesToChange.Count != m_rule.m_nodeDataList.Count)
        {
            Debug.LogWarning("rule cant be applied");
            for (int i = 0; i < m_nodesToChange.Count; i++)
            {
                m_nodesToChange[i].m_nodeData.symbol = m_rule.m_symbol;
                m_nodesToChange[i].m_nodeData.colour = Color.white; //reminder to set to default colour down the line? (or lock default to white)
            }
        }
        else
        {
            Debug.Log("rule applied");
        }
        m_nodesToChange.Clear();
    }

    private Vector2NodeDataLinker GetMatchingNodes()
    {
        Vector2NodeDataLinker node = null;
        int index = UnityEngine.Random.Range(0, m_matchingNodes.Count);
        
        if (0<m_matchingNodes.Count)
        {
            node = m_matchingNodes[index];
            m_originFoundIndex = node.m_index;
            if (1 < m_rule.m_nodeDataList.Count ) //was equal to one
                m_matchingNodes.Remove(node);

        }

        return node;
    }


}
