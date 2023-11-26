using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using static Graph;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateRule", order = 1)]
public class RuleScriptableObject : ScriptableObject
{
    [Serializable]
    public struct LeftHand
    {
        public Vector2 m_nodePosition;
        public char m_symbol;
    }
    [Serializable]
    public struct LeftHandEdge
    {
        public char m_symbol;
        public int m_fromNode;
        public int m_toNode;
        public bool m_directional;
    }

    [Header("Run Info")]
    public bool m_runOnce;
    public int m_maxIterations = 1;

    [Header("Left Hand")]
    public List<LeftHand> m_leftHand;
    public List<LeftHandEdge> m_leftHandEdge;

    [Header("Right Hand")]
    public List<NodeData> m_nodeDataList;

}

/* PLAN FOR EDGES
 * 
 * sort to/from setting
 * 
 * 
 * 
 * set from node when first node found
 * when second node from -> set to node
 * then replace from node for next index
 * repeat for rest
 * 
 * this will fill out edge list
 * after nodes have been placed 
 * replace list with RH edges
 * 
 * 
 */