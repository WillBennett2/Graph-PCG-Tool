using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Graph;

[CustomEditor(typeof(RuleScriptableObject))]
public class RuleEditor : Editor
{
    //public override void OnInspectorGUI()
    //{
    //    RuleScriptableObject ruleSO = (RuleScriptableObject)target;
    //    //rule.m_runOnce = EditorGUILayout.Toggle("Run Once", rule.m_runOnce);
    //    if (!ruleSO.m_runOnce && GraphInfo.m_graphInfo != null)
    //    {
    //        ruleSO.m_maxIterations = EditorGUILayout.IntSlider("Max Iterations", ruleSO.m_maxIterations, 1, GraphInfo.m_graphInfo.m_graphSize);
    //    }
    //    base.OnInspectorGUI();
    //}
}
