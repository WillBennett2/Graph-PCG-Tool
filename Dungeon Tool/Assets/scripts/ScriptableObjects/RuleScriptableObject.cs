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

    [Header("Run Info")]
    public bool m_runOnce;
    public int m_maxIterations = 1;

    [Header("Left Hand")]
    public Vector2 m_firstNodePos;
    public List<LeftHand> m_leftHand;

    [Header("Right Hand")]
    public List<NodeData> m_nodeDataList;

}
