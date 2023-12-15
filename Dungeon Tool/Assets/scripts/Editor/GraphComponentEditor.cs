using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphComponent))]
public class GraphComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GraphComponent graphComponent = (GraphComponent)target;

        if (GUILayout.Button("Run Rule."))
        {
            graphComponent.m_ruleReference.RunRule(graphComponent.m_nodes, graphComponent.m_storedNodes,graphComponent.m_edges);
            Debug.Log("Rule has been run");
        }
    }
}
