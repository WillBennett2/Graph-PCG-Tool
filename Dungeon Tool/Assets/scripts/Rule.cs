using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static Alphabet;
using static Graph;

[ExecuteInEditMode]
public class Rule : MonoBehaviour
{
    private Alphabet m_alphabet;
    [SerializeField] private RuleScriptableObject m_rule;
    private string m_orientation;
    [SerializeField] private int m_maxTries = 10;
    private List<Vector2NodeDataLinker> m_nodeGraph = new List<Vector2NodeDataLinker>();
    [SerializeField] private List<Vector2NodeDataLinker> m_matchingNodes = new List<Vector2NodeDataLinker>();
    private int m_originFoundIndex;
    [SerializeField]private List<Vector2NodeDataLinker> m_nodesToChange = new List<Vector2NodeDataLinker>();

    private List<Vector2EdgeDataLinker> m_edgeGraph = new List<Vector2EdgeDataLinker>();
    [SerializeField] private bool m_LoopNode = true;
    private int m_fromNodeIndex;
    private int m_toNodeIndex;
    private int m_edgeCount = 0;
    private int m_firstNodeIndex =-1;
    private int m_lastNodeIndex =-1;
    public RuleScriptableObject m_ruleRef { get; private set; }

    private void Awake()
    {
        m_ruleRef = m_rule;
        m_alphabet = GetComponent<Alphabet>();
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
        Debug.Log("orientation is "+m_orientation);
    }
    private Vector2 ChangeOrientation(Vector2 direction)
    {
        switch (m_orientation)
        {
            case ("Up"):
                direction = new Vector2(direction.x, direction.y);
                break;
            case ("Right"):
                direction = new Vector2(direction.y, direction.x*-1);
                break;
            case ("Left"):
                direction = new Vector2(direction.y*-1, direction.x);
                break;
            case ("Down"):
                direction = new Vector2(direction.x*-1, direction.y*-1);
                break;
        }
        return direction;
    }
    public void Replace(List<Vector2NodeDataLinker> nodes, List<Vector2EdgeDataLinker> edges)
    {
        m_matchingNodes.Clear();
        m_nodesToChange.Clear();
        //_rule.m_leftHandEdge.Clear();
        m_nodeGraph = nodes;
        m_edgeGraph = edges;
        Vector2NodeDataLinker matchingNode = null;

        PopulateMatchingNodes(nodes,0);

        for (int j = 0; j < m_maxTries; j++)
        {
            m_edgeCount = 0;
            for (int i = 0; i < m_rule.m_leftHand.Count; i++)
            {
                Debug.Log("This node = " + i);
                if (1 <= i)
                {
                    if (matchingNode != null)
                        SetFromNode(matchingNode.m_index);
                    matchingNode = GetNeighbouringNodes(i);
                    if (matchingNode != null)
                    {
                        m_nodesToChange.Add(matchingNode);
                        matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                        {
                            if (matchingNode.m_nodeData.symbol == data.m_symbol)
                                matchingNode.m_nodeData.colour = data.m_colour;
                        }
                        SetToNode(matchingNode.m_index);
                        m_lastNodeIndex = m_toNodeIndex;
                        if (m_toNodeIndex < m_fromNodeIndex)
                        {
                            int temp = m_fromNodeIndex;
                            m_fromNodeIndex = m_toNodeIndex;
                            m_toNodeIndex = temp;
                        }
                        SetEdge();
                    }

                }
                else if (1 < m_rule.m_leftHand.Count)
                {
                    matchingNode = GetMatchingNodes();
                    if (matchingNode != null)
                    {
                        m_nodesToChange.Add(matchingNode);
                        matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
                        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                        {
                            if (matchingNode.m_nodeData.symbol == data.m_symbol)
                                matchingNode.m_nodeData.colour = data.m_colour;
                        }
                        SetFromNode(matchingNode.m_index);
                        m_firstNodeIndex = m_fromNodeIndex;
                    }
                    SetOrientation();
                }
            }
            if (m_LoopNode && m_rule.m_leftHandEdge.Count== m_rule.m_leftHand.Count)
            {
                Debug.Log("first node index = " + m_firstNodeIndex);
                Debug.Log("last node index = " + m_lastNodeIndex);
                if (m_firstNodeIndex < m_lastNodeIndex)
                {
                    int temp = m_lastNodeIndex;
                    m_lastNodeIndex = m_firstNodeIndex;
                    m_firstNodeIndex = temp;
                }
                foreach (var edge in m_edgeGraph)
                {
                    if (edge.m_edgeData.to == m_firstNodeIndex)
                    {
                        if (edge.m_edgeData.from == m_lastNodeIndex)
                        {
                            if (edge.m_edgeData.symbol == m_rule.m_leftHandEdge[m_rule.m_leftHandEdge.Count - 1].m_symbol)
                            {
                                Debug.Log("setting last link edge");
                                SetFromNode(m_lastNodeIndex);
                                SetToNode(m_firstNodeIndex);
                                SetEdge();
                                break;
                            }
                        }
                    }
                }
            }
            if(ApplyChanges())
            {
                foreach (Vector2EdgeDataLinker edgeData in m_edgeGraph)
                {
                    for (int i = 0; i < m_rule.m_leftHandEdge.Count; i++)
                    {
                        if (edgeData.m_edgeData.from == m_rule.m_leftHandEdge[i].m_fromNode
                            && edgeData.m_edgeData.to == m_rule.m_leftHandEdge[i].m_toNode)
                        {
                            Debug.Log(edgeData.m_edgeData.from + " vs " + m_rule.m_leftHandEdge[i].m_fromNode);
                            Debug.Log(edgeData.m_edgeData.to + " vs " + m_rule.m_leftHandEdge[i].m_toNode);

                            edgeData.m_edgeData.symbol = m_rule.m_edgeDataList[i].symbol;
                            edgeData.m_edgeData.directional = m_rule.m_edgeDataList[i].directional;
                            foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                            {
                                if (edgeData.m_edgeData.symbol == data.m_symbol)
                                {
                                    edgeData.m_edgeData.colour = data.m_colour;
                                    Debug.Log("egde change done");
                                }
                            }
                        }
                    }

                }
                break;
            }
        }

        #region multiple_applications
        //if (!m_rule.m_runOnce)
        //{
        //    for (int j = 1; j <= m_rule.m_maxIterations; j++)
        //    {
        //        for (int i = 0; i < m_rule.m_nodeDataList.Count; i++)
        //        {
        //            if (i >= 1 && m_rule.m_nodeDataList.Count >= 1)
        //            {
        //                Debug.Log("loops done = " + i);
        //                matchingNode = GetNeighbouringNodes();
        //                if (matchingNode != null)
        //                {
        //                    m_nodesToChange.Add(matchingNode);
        //                    matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
        //                    matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
        //                }

        //                //reset
        //                m_matchingNodes.Clear();
        //                m_position = new Vector2(-1, -1);
        //                PopulateMatchingNodes(nodes);
        //            }
        //            else if (m_rule.m_nodeDataList.Count > 1)
        //            {

        //                matchingNode = GetMatchingNodes();
        //                m_nodesToChange.Add(matchingNode);
        //                matchingNode.m_nodeData.symbol = m_rule.m_nodeDataList[i].symbol;
        //                matchingNode.m_nodeData.colour = m_rule.m_nodeDataList[i].colour;
        //            }
        //        }
        //        ApplyChanges();
        //    }
        //}
        #endregion

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
        Vector2NodeDataLinker lastNode = m_nodeGraph[m_originFoundIndex]; //hint

        Vector2 directionToCheck = lastNode.m_nodeData.position + ChangeOrientation(m_rule.m_leftHand[index].m_nodePosition);

        node = CheckNode(m_nodeGraph,index ,directionToCheck);
        if(node != null)
        {
            m_originFoundIndex = node.m_index;
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
                foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                {
                    if(m_nodesToChange[i].m_nodeData.symbol == data.m_symbol)
                        m_nodesToChange[i].m_nodeData.colour = data.m_colour;
                }
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

    private void SetFromNode(int fromNode)
    {
        m_fromNodeIndex = fromNode;
        Debug.Log("from node set " + m_fromNodeIndex);
    }
    private void SetToNode(int toNode)
    {
        m_toNodeIndex = toNode;
        Debug.Log("to node set " + m_toNodeIndex);
    }
    private void SetEdge()
    {
        m_rule.m_leftHandEdge[m_edgeCount] = new RuleScriptableObject.LeftHandEdge(
            m_rule.m_leftHandEdge[m_edgeCount].m_symbol,
            m_fromNodeIndex,
            m_toNodeIndex,
            m_rule.m_leftHandEdge[m_edgeCount].m_directional
            );

        m_toNodeIndex = m_fromNodeIndex = -1;
        m_edgeCount++;
    }
}
