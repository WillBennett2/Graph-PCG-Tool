using System;
using System.Collections.Generic;
using UnityEngine;
using static Graph;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateRule", order = 1)]
public class RuleScriptableObject : ScriptableObject
{
    [Serializable]
    public struct LeftHandNode
    {
        public Vector2 m_nodePosition;
        public char m_symbol;
        public List<char> m_storedNodes;
    }
    [Serializable]
    public struct LeftHandEdge
    {
        public char m_symbol;
        public int m_fromNode;
        public int m_toNode;
        public bool m_directional;

        public LeftHandEdge(char symbol, int fromNode, int toNode, bool directional)
        {
            m_symbol = symbol;
            m_fromNode = fromNode;
            m_toNode = toNode;
            m_directional = directional;
        }
    }
    [Serializable]
    public struct RightHand
    {
        [Range(0, 1)] public float m_rightHandProbability;//multiple right hand options
        public List<NodeData> m_nodeDataList;
        public List<EdgeData> m_edgeDataList;
    }

    [Header("Run Info")]
    public bool m_runOnce = true;
    public int m_maxIterations = 1;
    [Range(0,1)]public float m_probability = 1;

    [Header("Left Hand")]
    public List<LeftHandNode> m_leftHand;
    public List<LeftHandEdge> m_leftHandEdge;

    [Header("Right Hand")]
    public List<RightHand> m_rightHand;
    //[Range(0, 1)] public float m_rightHandProbability = 1;//multiple right hand options
    //public List<NodeData> m_nodeDataList;
    //public List<EdgeData> m_edgeDataList;

}
