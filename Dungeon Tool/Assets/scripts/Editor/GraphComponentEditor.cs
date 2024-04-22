using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            graphComponent. Reset();
            bool generated = graphComponent.Generate();
            
            if(generated)
                Debug.Log("Map successfully generated");
            else
                Debug.Log("Map failed to generate");
        }

        if (GUILayout.Button("Reset."))
        {
            graphComponent.Reset();

            Debug.Log("Reset Run");
        }

        if (GUILayout.Button("Print Graph."))
        {
            graphComponent.PrintGraph();
            Debug.Log("Graph outputted.");
        }

        if (GUILayout.Button("Create Room Set SO"))
        {
            PreAuthoredRoomSO instance = ScriptableObject.CreateInstance<PreAuthoredRoomSO>();
            AssetDatabase.CreateAsset(instance, "Assets/scripts/RoomSet.asset");
            instance.m_roomSets = new List<PreAuthoredRoomSO.Roomset>();
            foreach (Alphabet.AlphabetLinker key in graphComponent.m_alphabet.m_alphabet)
            {
                PreAuthoredRoomSO.Roomset roomSet = new PreAuthoredRoomSO.Roomset();
                roomSet.m_alphabetKey = key.m_symbol;
                instance.m_roomSets.Add(roomSet);
            }
            instance.name = "RoomSet";
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;

            Debug.Log("Room set scriptable object outputted.");
        }
    }
}
