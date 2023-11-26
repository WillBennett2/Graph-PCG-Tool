using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Graph;

[CustomEditor(typeof(GraphComponent))]
public class GraphComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GraphComponent graphComponent = (GraphComponent)target;

        if(GUILayout.Button("Run Rule."))
        {
            graphComponent.m_ruleReference.Replace(graphComponent.m_nodes);
            Debug.Log("Rule has been run");
        }
    }
}
