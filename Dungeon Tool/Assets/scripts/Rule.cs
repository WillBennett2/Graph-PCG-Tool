using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;
using static Alphabet;
using static Graph;
using static RuleScriptableObject;

[ExecuteInEditMode]
public class Rule : MonoBehaviour
{
    private GraphSpace m_graphSpace;
    private Alphabet m_alphabet;
    [HideInInspector] private RuleScriptableObject m_rule;
    [SerializeField] private List<RuleScriptableObject> m_rules;
    private string m_orientation;
    [SerializeField] private int m_maxTries = 10;

    private int m_originFoundIndex;
    private List<Index2NodeDataLinker> m_nodeGraph = new List<Index2NodeDataLinker>();
    private List<Index2NodeDataLinker> m_matchingNodes = new List<Index2NodeDataLinker>();
    private List<Index2NodeDataLinker> m_nodesToChange = new List<Index2NodeDataLinker>();
    private List<Index2NodeDataLinker> m_nodeStore = new List<Index2NodeDataLinker>();

    private List<Index2StoredNodeDataLinker> m_storedNodesGraph;

    [Header("Edge Data")]
    private List<Index2EdgeDataLinker> m_edgeGraph = new List<Index2EdgeDataLinker>();
    private int m_fromNodeIndex;
    private int m_toNodeIndex;
    private int m_edgeCount = 0;
    private int m_firstNodeIndex = -1;
    private int m_lastNodeIndex = -1;
    public RuleScriptableObject m_ruleRef { get; private set; }

    private void Start()
    {
        m_ruleRef = m_rule;
        m_alphabet = GraphInfo.graphInfo.m_alphabet;
        m_graphSpace = GetComponent<GraphSpace>();
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
    private int GetRightHandRuleIndex(RuleScriptableObject rule)
    {
        List<RightHand> tempRightHand = new List<RightHand>(rule.m_rightHand);
        for (int i = 0; i < tempRightHand.Count; i++)
        {
            int index = Random.Range(0, tempRightHand.Count);
            if (Random.Range(0, 100) > tempRightHand[index].m_rightHandProbability * 100)
            {
                tempRightHand.Remove(tempRightHand[index]);
            }
            else
            {
                return index;
            }
        }
        return -1;
    }
    public void RunRule(List<Index2NodeDataLinker> nodes,List<Index2StoredNodeDataLinker> storedNodes ,List<Index2EdgeDataLinker> edges)
    {
        foreach (RuleScriptableObject rule in m_rules)
        {
            if (!rule.m_runOnce)
            {
                for (int x = 1; x <= rule.m_maxIterations; x++)
                {
                    Replace(nodes, storedNodes, edges, rule);
                }
            }
            else
            {
                if (Replace(nodes, storedNodes, edges, rule))
                {
                    
                }
            }
        }
        m_graphSpace.CreateSpace(nodes,storedNodes,edges);


    }
    private bool Replace(List<Index2NodeDataLinker> nodes, List<Index2StoredNodeDataLinker> storedNodes, List<Index2EdgeDataLinker> edges, RuleScriptableObject rule)
    {
        int rightHandIndex = GetRightHandRuleIndex(rule);
        if (!SetUp(nodes, storedNodes, edges, rule, rightHandIndex))
        {
            return false;
        }
        Index2NodeDataLinker matchingNode = null;
        for (int j = 0; j < m_maxTries; j++)
        {
            m_edgeCount = 0;
            for (int i = 0; i < rule.m_leftHand.Count; i++)
            {
                Debug.Log("This node = " + i);
                if (1 <= i)
                {
                    if (matchingNode != null)
                        SetFromNode(matchingNode.index);
                    matchingNode = GetNeighbouringNodes(rule, i);
                    if (matchingNode == null)
                    {
                        break;
                    }
                    SetNodeData(rule.m_rightHand[rightHandIndex], i, matchingNode);
                    SetToNode(matchingNode.index);

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
                    SetNodeData(rule.m_rightHand[rightHandIndex], i, matchingNode);
                    SetFromNode(matchingNode.index);
                    m_firstNodeIndex = m_fromNodeIndex;
                    SetOrientation();
                }
            }
            if (rule.m_rightHand[rightHandIndex].m_LoopNode && rule.m_leftHandEdge.Count == rule.m_leftHand.Count)
            {
                LoopEdge(rule);
            }

            if (ApplyNodeChanges(rule, rule.m_rightHand[rightHandIndex]))
            {
                ApplyEdgeChanges(rule, rule.m_rightHand[rightHandIndex]);

                //sort stored nodes
                ChangeStoredNodeData(rule, rightHandIndex);
                break;
            }
            m_nodesToChange.Clear();
        }
        return true;
    }
    private bool SetUp(List<Index2NodeDataLinker> nodes, List<Index2StoredNodeDataLinker> storedNodes, List<Index2EdgeDataLinker> edges, RuleScriptableObject rule, int rightHandIndex)
    {
        m_matchingNodes.Clear();
        m_nodesToChange.Clear();
        m_nodeStore.Clear();
        m_nodeGraph = nodes;
        m_storedNodesGraph = storedNodes;
        m_edgeGraph = edges;

        //find a right hand to use.
        if (rightHandIndex == -1)
        {
            Debug.Log("right hand rolled bad");
            return false;
        }
        if (Random.Range(0, 100) > rule.m_rightHand[rightHandIndex].m_rightHandProbability * 100)
        {
            Debug.Log(rule.name + " rolled bad");
            return false;
        }
        Debug.Log("Trying " + rule.name);
        PopulateMatchingNodes(rule, nodes, 0);
        if (m_matchingNodes.Count == 0)
        {
            Debug.LogWarning("No matching nodes found");
            return false;
        }

        return true;
    }

    private void PopulateMatchingNodes(RuleScriptableObject rule, List<Index2NodeDataLinker> nodes, int index)
    {
        foreach (Index2NodeDataLinker node in nodes)
        {
            if (node.nodeData.symbol == rule.m_leftHand[index].m_symbol
                && (node.nodeData.position == new Vector3(rule.m_leftHand[index].m_nodePosition.x, rule.m_leftHand[index].m_nodePosition.y)
                || rule.m_leftHand[index].m_nodePosition == new Vector2(-1, -1)))
            {
                m_matchingNodes.Add(node);
            }
        }

    }
    private Index2NodeDataLinker GetMatchingNodes()
    {
        Index2NodeDataLinker node = null;
        int index = Random.Range(0, m_matchingNodes.Count);

        if (0 < m_matchingNodes.Count)
        {
            node = m_matchingNodes[index];
            m_originFoundIndex = node.index;
        }

        return node;
    }
    private Index2NodeDataLinker GetNeighbouringNodes(RuleScriptableObject rule, int index)
    {
        Index2NodeDataLinker node = null;
        Index2NodeDataLinker lastNode = m_nodeGraph[m_originFoundIndex];

        Vector3 directionToCheck = lastNode.nodeData.position + ChangeOrientation(rule.m_leftHand[index].m_nodePosition * GraphInfo.graphInfo.m_scale);
        node = CheckNode(rule, m_nodeGraph, index, directionToCheck);
        if (node != null)
        {
            m_originFoundIndex = node.index;
        }
        else
        {
            Debug.Log("node doesnt match orientation");
        }
        return node;
    }
    private Vector3 ChangeOrientation(Vector3 direction)
    {
        switch (m_orientation)
        {
            case ("Up"):
                direction = new Vector3(direction.x, 0,direction.y);
                break;
            case ("Right"):
                direction = new Vector3(direction.y, 0,direction.x * -1);
                break;
            case ("Left"):
                direction = new Vector3(direction.y * -1, 0,direction.x);
                break;
            case ("Down"):
                direction = new Vector3(direction.x * -1, 0,direction.y * -1);
                break;
        }
        return direction;
    }
    private Index2NodeDataLinker CheckNode(RuleScriptableObject rule, List<Index2NodeDataLinker> graph, int index, Vector3 direction)
    {
        Index2NodeDataLinker node = null;
        Index2NodeDataLinker nodeToCheck = null;

        foreach (Index2NodeDataLinker vertex in graph)
        {
            if (vertex.nodeData.position == direction)
            {
                nodeToCheck = vertex;
            }
        }
        if (nodeToCheck != null && nodeToCheck.nodeData.symbol == rule.m_leftHand[index].m_symbol)
        {
            node = nodeToCheck;
        }

        return node;
    }
    private void SetNodeData(RightHand rightHand,int i, Index2NodeDataLinker matchingNode)
    {
        m_nodesToChange.Add(matchingNode);
        m_nodeStore.Add(matchingNode);
        matchingNode.nodeData.symbol = rightHand.m_nodeDataList[i].symbol;
        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
        {
            if (matchingNode.nodeData.symbol == data.m_symbol)
                matchingNode.nodeData.colour = data.m_colour;
        }
        matchingNode.nodeData.terrain = rightHand.m_nodeDataList[i].terrain;
        matchingNode.nodeData.preAuthored = rightHand.m_nodeDataList[i].preAuthored;
    }
    private void LoopEdge(RuleScriptableObject rule)
    {
        Debug.Log("from node = " + m_firstNodeIndex);
        Debug.Log("to node = " + m_lastNodeIndex);
        foreach (var edge in m_edgeGraph)
        {
            if (edge.edgeData.graphToNode == m_firstNodeIndex || edge.edgeData.graphToNode == m_lastNodeIndex)
            {
                if (edge.edgeData.graphFromNode == m_lastNodeIndex || edge.edgeData.graphFromNode == m_firstNodeIndex)
                {
                    if (edge.edgeData.symbol == rule.m_leftHandEdge[rule.m_leftHandEdge.Count - 1].m_symbol)
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
    private bool ApplyNodeChanges(RuleScriptableObject rule, RightHand rightHand)
    {
        bool applied = false;
        if (m_nodesToChange.Count != rightHand.m_nodeDataList.Count)
        {
            ResetRule(rule);
        }
        else
        {
            Debug.Log("node changes applied");
            applied = true;
        }
        return applied;
    }
    private void ApplyEdgeChanges(RuleScriptableObject rule, RightHand rightHand)
    {
        if (rule.m_leftHandEdge.Count != rightHand.m_edgeDataList.Count)
        {
            Debug.LogError("Left hand edges count do not match Right Hand edges count. Check rule");
            ResetRule(rule);
            return;
        }

        foreach (Index2EdgeDataLinker edgeData in m_edgeGraph)
        {
            for (int i = 0; i < rule.m_leftHandEdge.Count; i++)
            {
                if(rightHand.m_LoopNode == false && (i == rule.m_leftHandEdge.Count-1 && rule.m_leftHandEdge.Count>1))
                {
                    break;
                }    
                if ((edgeData.edgeData.graphFromNode == rule.m_leftHandEdge[i].m_fromNode
                    && edgeData.edgeData.graphToNode == rule.m_leftHandEdge[i].m_toNode)
                    || (edgeData.edgeData.graphToNode == rule.m_leftHandEdge[i].m_fromNode
                    && edgeData.edgeData.graphFromNode == rule.m_leftHandEdge[i].m_toNode)
                    )
                {
                    ChangeEdgeData(rule, rightHand, edgeData, i);
                }
            }

        }
        Debug.Log("edge changes applied");
    }
    private void ResetRule(RuleScriptableObject rule)
    {
        Debug.LogWarning("rule cant be applied");
        for (int i = 0; i < m_nodesToChange.Count; i++)
        {
            m_nodesToChange[i].nodeData.symbol = m_nodeStore[i].nodeData.symbol;
            foreach (AlphabetLinker data in m_alphabet.m_alphabet)
            {
                if (m_nodesToChange[i].nodeData.symbol == data.m_symbol)
                    m_nodesToChange[i].nodeData.colour = data.m_colour;
            }
            m_nodesToChange[i].nodeData.terrain = m_nodeStore[i].nodeData.terrain;
            m_nodesToChange[i].nodeData.preAuthored = m_nodeStore[i].nodeData.preAuthored;
        }
        m_nodeStore.Clear();
    }
    private void ChangeEdgeData(RuleScriptableObject rule, RightHand rightHand, Index2EdgeDataLinker edgeData, int i)
    {
        edgeData.edgeData.symbol = rightHand.m_edgeDataList[i].symbol;
        edgeData.edgeData.directional = rightHand.m_edgeDataList[i].directional;
        edgeData.edgeData.terrian = rightHand.m_edgeDataList[i].terrian;
        edgeData.edgeData.fromNode = rule.m_leftHandEdge[i].m_fromNode;
        edgeData.edgeData.toNode = rule.m_leftHandEdge[i].m_toNode;

        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
        {
            if (edgeData.edgeData.symbol == data.m_symbol)
            {
                edgeData.edgeData.colour = data.m_colour;
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
        rule.m_leftHandEdge[m_edgeCount] = new LeftHandEdge(
            rule.m_leftHandEdge[m_edgeCount].m_symbol,
            m_fromNodeIndex,
            m_toNodeIndex,
            rule.m_leftHandEdge[m_edgeCount].m_directional
            );

        m_toNodeIndex = m_fromNodeIndex = -1;
        m_edgeCount++;
    }

    private void ChangeStoredNodeData(RuleScriptableObject rule, int rightHandIndex)
    {
        for (int i = 0; i < m_nodesToChange.Count; i++)
        {
            foreach (StoredNodeData storedNode in rule.m_rightHand[rightHandIndex].m_nodeDataList[i].storedNodes)
            {
                GraphInfo.graphInfo.nodeIndexCounter++;
                Index2StoredNodeDataLinker newStoredNode = new Index2StoredNodeDataLinker(GraphInfo.graphInfo.nodeIndexCounter, storedNode);
                newStoredNode.storedNodeData.parentIndex = m_nodesToChange[i].index;
                float randomPosMod = Random.Range(-0.25f, 0.25f);
                newStoredNode.storedNodeData.position = new Vector3(m_nodesToChange[i].nodeData.position.x + randomPosMod,1f,m_nodesToChange[i].nodeData.position.z + randomPosMod);
                foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                {
                    if (newStoredNode.storedNodeData.symbol == data.m_symbol)
                    {
                        newStoredNode.storedNodeData.colour = data.m_colour;
                    }
                }
                m_storedNodesGraph.Add(newStoredNode);

                EdgeData edgeData = new EdgeData();
                edgeData.symbol = "e";
                foreach (AlphabetLinker data in m_alphabet.m_alphabet)
                {
                    if (edgeData.symbol == data.m_symbol)
                    {
                        edgeData.colour = data.m_colour;
                    }
                }
                edgeData.directional = true;
                edgeData.fromNode = GraphInfo.graphInfo.nodeIndexCounter;
                edgeData.toNode = m_nodesToChange[i].index;
                edgeData.graphFromNode = GraphInfo.graphInfo.nodeIndexCounter;
                edgeData.graphToNode = m_nodesToChange[i].index;
                edgeData.position = new Vector3(newStoredNode.storedNodeData.position.x, newStoredNode.storedNodeData.position.y, newStoredNode.storedNodeData.position.z);
                Index2EdgeDataLinker newEdge = new Index2EdgeDataLinker(GraphInfo.graphInfo.edges.Count, edgeData);

                m_edgeGraph.Add(newEdge);
            }
        }
    }
}
