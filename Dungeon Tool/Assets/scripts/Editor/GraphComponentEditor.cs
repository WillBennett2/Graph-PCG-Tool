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
            graphComponent.Generate();
            
            Debug.Log("Rule has been run");
        }

        if (GUILayout.Button("Print Graph."))
        {
            graphComponent.PrintGraph();
            Debug.Log("Graph outputted.");
        }
    }
}
