using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UnityEngine;
using static Alphabet;
using static Graph;
using static RuleScriptableObject;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class Rule
{
    public static event Action<bool> OnRuleApplied;
    private Alphabet m_alphabet;
    private RuleScriptableObject m_rule;
    private List<RuleScriptableObject> m_rules;
    private string m_orientation;
    private int m_maxTries = 10;

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

    public Rule()
    {
        OnEnable();
        Start();
    }
    private void OnEnable()
    {
        GraphComponent.OnClearData += Clear;
        GraphComponent.OnDisableScripts += OnDisable;
        GraphComponent.OnSetRecipe += SetRecipe;
        GraphComponent.OnRunGraphGrammar += RunRule;
    }
    private void OnDisable()
    {
        GraphComponent.OnClearData -= Clear;
        GraphComponent.OnSetRecipe -= SetRecipe;
        GraphComponent.OnRunGraphGrammar -= RunRule;
        GraphComponent.OnDisableScripts -= OnDisable;
    }
    private void Start()
    {
        m_ruleRef = m_rule;
        m_alphabet = GraphInfo.graphInfo.m_alphabet;
    }
    private void Clear()
    {
        m_nodeGraph.Clear();
        m_matchingNodes.Clear();
        m_nodesToChange.Clear();
        m_nodeStore.Clear();
        m_edgeGraph.Clear();
    }
    private void SetRecipe(List<RuleScriptableObject> rules, int maxTries)
    {
        m_rules = rules;
        m_maxTries = maxTries;
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
    public void RunRule(List<Index2NodeDataLinker> nodes, List<Index2StoredNodeDataLinker> storedNodes, List<Index2EdgeDataLinker> edges)
    {
        bool ruleRun = false;
        foreach (RuleScriptableObject rule in m_rules)
        {
            if (!rule.m_runOnce)
            {
                for (int x = 1; x <= rule.m_maxIterations; x++)
                {
                    if (Replace(nodes, storedNodes, edges, rule))
                        ruleRun = true;
                }
            }
            else
            {
                if (Replace(nodes, storedNodes, edges, rule))
                {
                    ruleRun = true;
                }
            }
        }

        OnRuleApplied?.Invoke(ruleRun);
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
            //m_nodeStore.Clear();
            m_edgeCount = 0;
            for (int i = 0; i < rule.m_leftHand.Count; i++)
            {
                //Debug.Log("This node = " + i);
                if (1 <= i)
                {
                    if (matchingNode != null)
                        SetFromNode(matchingNode.index);
                    matchingNode = GetNeighbouringNodes(rule, i);
                    if (matchingNode == null)
                    {
                        //ResetRule(rule);
                        Debug.Log("match node is null");
                        break;
                    }
                    SetNodeData(rule.m_rightHand[rightHandIndex], i, matchingNode);
                    SetToNode(matchingNode.index);

                    m_lastNodeIndex = m_toNodeIndex;
                    SetEdge(rule);

                }
                else if (rule.m_leftHand.Count == 1)
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
                Debug.Log(rule.name + " has been applied");
                return true;
            }
            else
            {
                ResetRule(rule);
            }
            m_nodesToChange.Clear();
        }
        return false;
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
                && (node.nodeData.gridCoordinates == new Vector2(rule.m_leftHand[index].m_nodeGridCoord.x, rule.m_leftHand[index].m_nodeGridCoord.y)
                || rule.m_leftHand[index].m_nodeGridCoord == new Vector2(-1, -1)))
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
        Vector2 lastNodeGridCoord = lastNode.nodeData.gridCoordinates;
        Vector2 directionToCheck = new Vector2(-1,-1);
        if (rule.m_leftHand[index].m_nodeGridCoord == new Vector2(-1,-1))
        {

            for (int i = 0; i < 4; i++)//check each direction
            {
                switch (i)
                {
                    case 0://check left
                        directionToCheck = lastNodeGridCoord + new Vector2(-1, 0); break;
                    case 1://check up
                        directionToCheck = lastNodeGridCoord + new Vector2(0, 1); break;
                    case 2://check right
                        directionToCheck = lastNodeGridCoord + new Vector2(1, 0); break;
                    case 3://check down
                        directionToCheck = lastNodeGridCoord + new Vector2(-0, -1); break;
                }
                node = CheckNode(rule, m_nodeGraph, index, directionToCheck);
                if(node!=null)
                        break;
            }


        }
        else
        {
            directionToCheck = lastNodeGridCoord + ChangeOrientation(rule.m_leftHand[index].m_nodeGridCoord);
            node = CheckNode(rule, m_nodeGraph, index, directionToCheck);
        }

        
        if(node!=null)
        {
            m_originFoundIndex = node.index;
        }
        else
        {
            Debug.Log("node doesnt match orientation");
        }
        return node;
    }
    private Vector2 ChangeOrientation(Vector3 direction)
    {
        switch (m_orientation)
        {
            case ("Up"):
                direction = new Vector3(direction.x,  direction.y);
                break;
            case ("Right"):
                direction = new Vector3(direction.y,  direction.x * -1);
                break;
            case ("Left"):
                direction = new Vector3(direction.y * -1, direction.x);
                break;
            case ("Down"):
                direction = new Vector3(direction.x * -1, direction.y * -1);
                break;
        }
        return direction;
    }
    private Index2NodeDataLinker CheckNode(RuleScriptableObject rule, List<Index2NodeDataLinker> graph, int index, Vector2 direction)
    {
        Index2NodeDataLinker node = null;
        Index2NodeDataLinker nodeToCheck = null;

        foreach (Index2NodeDataLinker vertex in graph)
        {
            if (vertex.nodeData.gridCoordinates == direction)
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
    private void SetNodeData(RightHand rightHand, int i, Index2NodeDataLinker matchingNode)
    {
        m_nodesToChange.Add(matchingNode);
        Index2NodeDataLinker nodeData = new Index2NodeDataLinker(matchingNode.index, matchingNode.nodeData);
        m_nodeStore.Add(nodeData);
        //m_nodeStore.Add(matchingNode);

        matchingNode.nodeData.symbol = rightHand.m_nodeDataList[i].symbol;
        foreach (AlphabetLinker data in m_alphabet.m_alphabet)
        {
            if (matchingNode.nodeData.symbol == data.m_symbol)
                matchingNode.nodeData.colour = data.m_colour;
        }
        matchingNode.nodeData.terrain = rightHand.m_nodeDataList[i].terrain;
        matchingNode.nodeData.difficultyRating = rightHand.m_nodeDataList[i].difficultyModifier;
        matchingNode.nodeData.difficultyInterval = rightHand.m_nodeDataList[i].difficultyInterval;
        matchingNode.nodeData.preAuthored = rightHand.m_nodeDataList[i].preAuthored;
    }
    private void LoopEdge(RuleScriptableObject rule)
    {
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
            //Debug.Log("node changes applied");
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
                if (rightHand.m_LoopNode == false && (i == rule.m_leftHandEdge.Count - 1 && rule.m_leftHandEdge.Count > 1))
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
        //Debug.Log("edge changes applied");
    }
    private void ResetRule(RuleScriptableObject rule)
    {
        Debug.LogWarning(rule.name + " can't be applied");
        if (m_nodeStore.Count == 0)
            return;
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
            m_nodesToChange[i].nodeData.difficultyRating = m_nodeStore[i].nodeData.difficultyRating;
            m_nodesToChange[i].nodeData.difficultyInterval = m_nodeStore[i].nodeData.difficultyInterval;
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
                newStoredNode.storedNodeData.position = new Vector3(m_nodesToChange[i].nodeData.position.x + randomPosMod, 1f, m_nodesToChange[i].nodeData.position.z + randomPosMod);
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
