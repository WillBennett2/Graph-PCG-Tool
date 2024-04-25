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
        public string m_symbol;
        //public List<char> m_storedNodes;
    }
    [Serializable]
    public struct LeftHandEdge
    {
        public string m_symbol;
        public int m_fromNode;
        public int m_toNode;
        public bool m_directional;

        public LeftHandEdge(string symbol, int fromNode, int toNode, bool directional)
        {
            m_symbol = symbol;
            m_fromNode = fromNode;
            m_toNode = toNode;
            m_directional = directional;
        }
    }
    [Serializable]
    public struct RightHandStoredNodeData
    {
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Color colour;
        public int index;
        public string symbol;
        public int parentIndex;
        public int terrain;
        public int item;
        public int enemy;

        public void SetColour(Color colour)
        {
            this.colour = colour;
        }
        public void SetIndex(int graphIndex)
        {
            index = graphIndex;
        }
        public void SetParentIndex(int index)
        {
            parentIndex = index;
            SetPosition();
        }
        private void SetPosition()
        {
            position = GraphInfo.graphInfo.nodes[parentIndex].nodeData.position;
        }
    }
    [Serializable]
    public struct RightHandNodeData
    {
        public string symbol;
        public Vector3 position;
        [HideInInspector] public Color colour;
        public int terrain;
        public bool preAuthored;
        [Tooltip("Leave at 0 to apply no modifier")]
        public int difficultyModifier;
        public int difficultyInterval;
        public List<StoredNodeData> storedNodes;
    }
    [Serializable]
    public struct RightHand
    {
        [Range(0, 1)] public float m_rightHandProbability;
        public bool m_LoopNode;
        public List<RightHandNodeData> m_nodeDataList;
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

}
