using System.Collections.Generic;
using UnityEngine;
using static Alphabet;
using static Graph;

[ExecuteInEditMode]
public class Rule : MonoBehaviour
{
    private Alphabet m_alphabet;
    [HideInInspector] private RuleScriptableObject m_rule;
    [SerializeField] private List<RuleScriptableObject> m_rules;
    private string m_orientation;
    [SerializeField] private int m_maxTries = 10;
    [Header("Node Data")]
    private int m_originFoundIndex;
    private List<Index2NodeDataLinker> m_nodeGraph = new List<Index2NodeDataLinker>();
    [SerializeField] private List<Index2NodeDataLinker> m_matchingNodes = new List<Index2NodeDataLinker>();
    [SerializeField] private List<Index2NodeDataLinker> m_nodesToChange = new List<Index2NodeDataLinker>();

    [Header("Edge Data")]
    private List<Index2EdgeDataLinker> m_edgeGraph = new List<Index2EdgeDataLinker>();
    [SerializeField] private bool m_LoopNode = true;
    private int m_fromNodeIndex;
    private int m_toNodeIndex;
    private int m_edgeCount = 0;
    private int m_firstNodeIndex = -1;
    private int m_lastNodeIndex = -1;
    public RuleScriptableObject m_ruleRef { get; private set; }

    private void Awake()
    {
        m_ruleRef = m_rule;
        m_alphabet = GetComponent<Alphabet>();
    }

    private void SetOrientation()
    {
        int direction = Random.Range(0, 4);
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
    public void Replace(List<Index2NodeDataLinker> nodes, List<Index2EdgeDataLinker> edges)
    {
        foreach (RuleScriptableObject rule in m_rules)
        {
            m_matchingNodes.Clear();
            m_nodesToChange.Clear();
            m_nodeGraph = nodes;
            m_edgeGraph = edges;
            Index2NodeDataLinker matchingNode = null;

            if(Random.Range(0, 100) > rule.m_rightHandProbability*100)
            {
                Debug.Log(rule.name + " rolled bad");
                break;
            }

            PopulateMatchingNodes(rule, nodes, 0);
            for (int j = 0; j < m_maxTries; j++)
            {
                m_edgeCount = 0;
                for (int i = 0; i < rule.m_leftHand.Count; i++)
                {
                    Debug.Log("This node = " + i);
                    if (1 <= i)
                    {

                        if (matchingNode != null)
                            SetFromNode(matchingNode.m_index);
                        matchingNode = GetNeighbouringNodes(rule, i);
                        if (matchingNode == null)
                        {
                            break;
                        }
                        SetNodeData(i, rule, matchingNode);
                        SetToNode(matchingNode.m_index);

                        m_lastNodeIndex = m_toNodeIndex;
                        SetEdge(rule);

                    }
                    else if (1 < rule.m_leftHand.Count)
                    {
                        matchingNode = GetMatchingNodes();
                        if (matchingNode == null)
                        {
                            break;
                        }
                        SetNodeData(i, rule, matchingNode);
                        SetFromNode(matchingNode.m_index);
                        m_firstNodeIndex = m_fromNodeIndex;
                        SetOrientation();
                    }
                }
                if (m_LoopNode && rule.m_leftHandEdge.Count == rule.m_leftHand.Count)
                {
                    LoopEdge(rule);
                }
                if (ApplyNodeChanges(rule))
                {
                    ApplyEdgeChanges(rule);

                    //sort stored nodes
                    for (int i = 0; i < m_nodesToChange.Count; i++)
                    {
                        foreach (StoredNodeData storedNode in rule.m_nodeDataList[i].storedNodes)
                        {
                            storedNode.SetParentIndex(m_nodesToChange[i].m_index);
                            GraphInfo.m_graphInfo.m_nodeIndexCounter++;
                            storedNode.SetIndex(GraphInfo.m_graphInfo.m_nodeIndexCounter);
                            m_nodesToChange[i].m_nodeData.storedNodes.Add(storedNode);
                        }
                    }

                    break;
                }
                m_nodesToChange.Clear();
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
    private void SetNodeData(int i, RuleScriptableObject rule, Index2NodeDataLinker matchingNode)
    {
        m_nodesToChange.Add(matchingNode);
        matchingNode.m_nodeData.symbol = rule.m_nodeDataList[i].symbol;
        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
        {
            if (matchingNode.m_nodeData.symbol == data.m_symbol)
                matchingNode.m_nodeData.colour = data.m_colour;
        }
    }
    private void LoopEdge(RuleScriptableObject rule)
    {
        Debug.Log("from node = " + m_firstNodeIndex);
        Debug.Log("to node = " + m_lastNodeIndex);
        foreach (var edge in m_edgeGraph)
        {
            if (edge.m_edgeData.toNode == m_firstNodeIndex || edge.m_edgeData.toNode == m_lastNodeIndex)
            {
                if (edge.m_edgeData.fromNode == m_lastNodeIndex || edge.m_edgeData.fromNode == m_firstNodeIndex)
                {
                    if (edge.m_edgeData.symbol == rule.m_leftHandEdge[rule.m_leftHandEdge.Count - 1].m_symbol)
                    {
                        Debug.Log("setting last link edge");
                        SetFromNode(m_lastNodeIndex);
                        SetToNode(m_firstNodeIndex);
                        SetEdge(rule);
                        break;
                    }
                }
            }
        }
    }

    private void PopulateMatchingNodes(RuleScriptableObject rule, List<Index2NodeDataLinker> nodes, int index)
    {
        foreach (Index2NodeDataLinker node in nodes)
        {
            if (node.m_nodeData.symbol == rule.m_leftHand[index].m_symbol
                && (node.m_nodeData.position == rule.m_leftHand[index].m_nodePosition
                || rule.m_leftHand[index].m_nodePosition == new Vector2(-1, -1)))
            {
                m_matchingNodes.Add(node);
            }
        }

    }
    private Index2NodeDataLinker GetNeighbouringNodes(RuleScriptableObject rule, int index)
    {
        Index2NodeDataLinker node = null;
        Index2NodeDataLinker lastNode = m_nodeGraph[m_originFoundIndex]; //hint

        Vector2 directionToCheck = lastNode.m_nodeData.position + ChangeOrientation(rule.m_leftHand[index].m_nodePosition);

        node = CheckNode(rule, m_nodeGraph, index, directionToCheck);
        if (node != null)
        {
            m_originFoundIndex = node.m_index;
        }
        else
        {
            Debug.Log("node doesnt match orientation");
        }
        return node;
    }
    private Index2NodeDataLinker CheckNode(RuleScriptableObject rule, List<Index2NodeDataLinker> graph, int index, Vector2 direction)
    {
        Index2NodeDataLinker node = null;
        Index2NodeDataLinker nodeToCheck = null;

        foreach (Index2NodeDataLinker vertex in graph)
        {
            if (vertex.m_nodeData.position == direction)
            {
                nodeToCheck = vertex;
            }
        }
        if (nodeToCheck != null && nodeToCheck.m_nodeData.symbol == rule.m_leftHand[index].m_symbol)
        {
            node = nodeToCheck;
        }

        return node;
    }
    private Index2NodeDataLinker GetMatchingNodes()
    {
        Index2NodeDataLinker node = null;
        int index = UnityEngine.Random.Range(0, m_matchingNodes.Count);

        if (0 < m_matchingNodes.Count)
        {
            node = m_matchingNodes[index];
            m_originFoundIndex = node.m_index;
        }

        return node;
    }
    private bool ApplyNodeChanges(RuleScriptableObject rule)
    {
        bool applied = false;
        List<StoredNodeData> storedNodesToDelete = new List<StoredNodeData>();
        if (m_nodesToChange.Count != rule.m_nodeDataList.Count)
        {
            Debug.LogWarning("rule cant be applied");
            for (int i = 0; i < m_nodesToChange.Count; i++)
            {
                m_nodesToChange[i].m_nodeData.symbol = rule.m_leftHand[i].m_symbol;
                foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                {
                    if (m_nodesToChange[i].m_nodeData.symbol == data.m_symbol)
                        m_nodesToChange[i].m_nodeData.colour = data.m_colour;
                }
            }
        }
        else
        {
            Debug.Log("rule applied");
            applied = true;
        }
        return applied;
    }
    private void ApplyEdgeChanges(RuleScriptableObject rule)
    {
        foreach (Index2EdgeDataLinker edgeData in m_edgeGraph)
        {
            for (int i = 0; i < rule.m_leftHandEdge.Count; i++)
            {
                if (edgeData.m_edgeData.fromNode == rule.m_leftHandEdge[i].m_fromNode
                    && edgeData.m_edgeData.toNode == rule.m_leftHandEdge[i].m_toNode)
                {
                    ChangeNodeData(rule, edgeData, i);
                }
                else if (edgeData.m_edgeData.toNode == rule.m_leftHandEdge[i].m_fromNode
                    && edgeData.m_edgeData.fromNode == rule.m_leftHandEdge[i].m_toNode)
                {
                    ChangeNodeData(rule, edgeData, i);
                }
            }

        }
    }
    private void ChangeNodeData(RuleScriptableObject rule, Index2EdgeDataLinker edgeData, int i)
    {
        edgeData.m_edgeData.symbol = rule.m_edgeDataList[i].symbol;
        edgeData.m_edgeData.directional = rule.m_edgeDataList[i].directional;
        edgeData.m_edgeData.directionalFromNode = rule.m_leftHandEdge[i].m_fromNode;
        edgeData.m_edgeData.directionalToNode = rule.m_leftHandEdge[i].m_toNode;

        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
        {
            if (edgeData.m_edgeData.symbol == data.m_symbol)
            {
                edgeData.m_edgeData.colour = data.m_colour;
            }
        }
    }
    private void SetFromNode(int fromNode)
    {
        m_fromNodeIndex = fromNode; ;
    }
    private void SetToNode(int toNode)
    {
        m_toNodeIndex = toNode;
    }
    private void SetEdge(RuleScriptableObject rule)
    {
        rule.m_leftHandEdge[m_edgeCount] = new RuleScriptableObject.LeftHandEdge(
            rule.m_leftHandEdge[m_edgeCount].m_symbol,
            m_fromNodeIndex,
            m_toNodeIndex,
            rule.m_leftHandEdge[m_edgeCount].m_directional
            );

        m_toNodeIndex = m_fromNodeIndex = -1;
        m_edgeCount++;
    }
}
