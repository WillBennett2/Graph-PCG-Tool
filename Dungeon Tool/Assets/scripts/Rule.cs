using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
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
    private string m_orientation;
    [SerializeField] private int m_maxTries = 10;
    private List<Vector2NodeDataLinker> m_graph = new List<Vector2NodeDataLinker>();
    [SerializeField] private List<Vector2NodeDataLinker> m_matchingNodes = new List<Vector2NodeDataLinker>();
    [SerializeField] private List<Vector2NodeDataLinker> m_matchingDirectionNodes = new List<Vector2NodeDataLinker>();
    private int m_originFoundIndex;
    [SerializeField] private List<Vector2NodeDataLinker> m_nodesToChange = new List<Vector2NodeDataLinker>();

    public RuleScriptableObject m_ruleRef { get; private set; }

    private void Awake()
    {
        m_ruleRef = m_rule;
    }

    private void SetOrientation()
    {
        int direction = UnityEngine.Random.Range(0, 4);
        switch (direction)
        {
            case (0):
                m_orientation = "Up";
                break;
            case (1):
                m_orientation = "Right";
                break;
            case (2):
                m_orientation = "Down";
                break;
            case (3):
                m_orientation = "Left";
                break;
        }
        Debug.Log("orientation is " + m_orientation);
    }
    private Vector2 ChangeOrientation(Vector2 direction)
    {
        switch (m_orientation)
        {
            case ("Up"):
                direction = new Vector2(direction.x, direction.y);
                break;
            case ("Right"):
                direction = new Vector2(direction.y, direction.x * -1);
                break;
            case ("Left"):
                direction = new Vector2(direction.y * -1, direction.x);
                break;
            case ("Down"):
                direction = new Vector2(direction.x * -1, direction.y * -1);
                break;
        }


        return direction;
    }
    public void Replace(List<Vector2NodeDataLinker> nodes)
    {
        m_matchingNodes.Clear();
        m_graph = nodes;
        Vector2NodeDataLinker matchingNode = null;
        bool ruleDone = false;

        PopulateMatchingNodes(nodes, 0);

        for (int n = 0; n < m_maxTries; n++)
        {
            if (m_rule.m_runOnce)
            {
                for (int i = 0; i < m_rule.m_leftHand.Count; i++)
                {
                    Debug.Log("This node = " + i);
                    if (1 <= i)
                    {
                        matchingNode = GetNeighbouringNodes(i);
                        if (matchingNode != null)
                        {
                            m_nodesToChange.Add(matchingNode);
                            matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                            matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                        }
                    }
                    else if (1 < m_rule.m_leftHand.Count)
                    {
                        matchingNode = GetMatchingNodes();
                        if (matchingNode != null)
                        {
                            m_nodesToChange.Add(matchingNode);
                            matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                            matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                        }
                        SetOrientation();
                    }
                }

                if (ApplyChanges())
                {
                    break;
                }
            }
            else
            {
                for (int j = 1; j <= m_rule.m_maxIterations; j++)
                {
                    for (int i = 0; i < m_rule.m_leftHand.Count; i++)
                    {
                        Debug.Log("This node = " + i);
                        if (1 <= i)
                        {
                            matchingNode = GetNeighbouringNodes(i);
                            if (matchingNode != null)
                            {
                                m_nodesToChange.Add(matchingNode);
                                matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                                matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                            }
                        }
                        else if (1 < m_rule.m_leftHand.Count)
                        {
                            matchingNode = GetMatchingNodes();
                            if (matchingNode != null)
                            {
                                m_nodesToChange.Add(matchingNode);
                                matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                                matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
                            }
                            SetOrientation();
                        }
                    }
                    if (!ApplyChanges())
                    {
                        j--;
                    }
                    else
                    {
                        n = m_maxTries;
                    }

                }
            }
        }
        


    }
    private void PopulateMatchingNodes(List<Vector2NodeDataLinker> nodes, int index)
    {
        foreach (Vector2NodeDataLinker node in nodes)
        {
            if (node.m_nodeData.symbol == m_rule.m_leftHand[index].m_symbol 
                && (node.m_nodeData.position == m_rule.m_leftHand[index].m_nodePosition 
                || m_rule.m_leftHand[index].m_nodePosition == new Vector2(-1, -1) ))
            {
                m_matchingNodes.Add(node);
            }
        }

    }
    private Vector2NodeDataLinker GetNeighbouringNodes(int index)
    {
        Vector2NodeDataLinker node = null;
        Vector2NodeDataLinker lastNode = m_graph[m_originFoundIndex]; //hint

        Vector2 directionToCheck = lastNode.m_nodeData.position + ChangeOrientation(m_rule.m_leftHand[index].m_nodePosition);

        node = CheckNode(m_graph,index ,directionToCheck);
        if(node != null)
        {
            Debug.Log("old index =" + m_originFoundIndex);
            m_originFoundIndex = node.m_index;
            Debug.Log("new index =" + m_originFoundIndex);
        }
        else
        {
            Debug.Log("node doesnt match orientation");
        }
        return node;
    }
    private Vector2NodeDataLinker CheckNode(List<Vector2NodeDataLinker> graph,int index ,Vector2 direction)
    {
        Vector2NodeDataLinker node = null;
        Vector2NodeDataLinker nodeToCheck = null;

        foreach (Vector2NodeDataLinker vertex in graph)
        {
            if (vertex.m_nodeData.position == direction)
            {
                nodeToCheck = vertex;
            }
        }
        if (nodeToCheck!=null && nodeToCheck.m_nodeData.symbol == m_rule.m_leftHand[index].m_symbol)
        {
            node = nodeToCheck;
        }

        return node;
    }
    private Vector2NodeDataLinker GetMatchingNodes()
    {
        Vector2NodeDataLinker node = null;
        int index = UnityEngine.Random.Range(0, m_matchingNodes.Count);

        if (0 < m_matchingNodes.Count)
        {
            node = m_matchingNodes[index];
            m_originFoundIndex = node.m_index;
            //if (1 < m_rule.m_nodeDataList.Count) //was equal to one
                //m_matchingNodes.Remove(node);

        }

        return node;
    }
    private bool ApplyChanges()
    {
        bool applied = false;
        if (m_nodesToChange.Count != m_rule.m_nodeDataList.Count)
        {
            Debug.LogWarning("rule cant be applied");
            for (int i = 0; i < m_nodesToChange.Count; i++)
            {
                m_nodesToChange[i].m_nodeData.symbol = m_rule.m_leftHand[i].m_symbol;
                m_nodesToChange[i].m_nodeData.colour = Color.white; //reminder to set to default colour down the line? (or lock default to white)
            }
        }
        else
        {
            Debug.Log("rule applied");
            applied = true;
        }
        m_nodesToChange.Clear();

        return applied;
    }


}
