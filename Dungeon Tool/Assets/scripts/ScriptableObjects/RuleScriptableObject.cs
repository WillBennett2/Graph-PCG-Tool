using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Graph;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateRule", order = 1)]
public class RuleScriptableObject : ScriptableObject
{
    public bool m_runOnce;
    public int m_maxIterations = 1;
    public Vector2 m_nodePosition = -Vector2.one;
    public char m_symbol = ' ';
    public List<NodeData> m_nodeDataList;

}
