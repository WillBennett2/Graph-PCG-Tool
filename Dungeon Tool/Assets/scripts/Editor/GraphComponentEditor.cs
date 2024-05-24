using OpenCover.Framework.Model;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphComponent))]
[CanEditMultipleObjects]
public class GraphComponentEditor : Editor
{

    SerializedProperty m_useRandom;
    SerializedProperty m_applyCurve;
    SerializedProperty m_applyIntervalValue;
    SerializedProperty m_usePoisson;
    SerializedProperty m_useJitter;

    private void OnEnable()
    {
        m_useRandom = serializedObject.FindProperty("m_useRandom");
        m_applyCurve = serializedObject.FindProperty("m_applyCurve");
        m_applyIntervalValue = serializedObject.FindProperty("m_applyIntervalValue");
        m_usePoisson = serializedObject.FindProperty("m_usePoisson");
        m_useJitter = serializedObject.FindProperty("m_useJitter");
    }
    public override void OnInspectorGUI()
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);

        GraphComponent graphComponent = (GraphComponent)target;

        EditorGUILayout.LabelField("Graph Values", headerStyle, GUILayout.Width(100));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rows", GUILayout.Width(50));
        graphComponent.m_rows = EditorGUILayout.IntField(graphComponent.m_rows, GUILayout.Width(30));
        GUILayout.Space(60);
        EditorGUILayout.LabelField("Columns", GUILayout.Width(70));
        graphComponent.m_columns = EditorGUILayout.IntField(graphComponent.m_columns, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Default Symbol", GUILayout.Width(100));
        graphComponent.m_defaultSymbol = EditorGUILayout.TextField(graphComponent.m_defaultSymbol, GUILayout.Width(30));
        GUILayout.Space(30f);
        EditorGUILayout.LabelField("Scale", GUILayout.Width(50));
        graphComponent.m_scale = EditorGUILayout.IntField(graphComponent.m_scale, GUILayout.Width(30));
        GUILayout.Space(30f);
        EditorGUILayout.LabelField("Offset", GUILayout.Width(50));
        graphComponent.m_offset = EditorGUILayout.IntField(graphComponent.m_offset, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Data", headerStyle);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Alphabet", GUILayout.Width(100));
        graphComponent.m_alphabet = (Alphabet)EditorGUILayout.ObjectField(graphComponent.m_alphabet, typeof(Class));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_nodes"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_edges"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_storedNodes"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pathList"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Rules", headerStyle);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_rules"), true);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max Tries", GUILayout.Width(100));
        graphComponent.m_maxTries = EditorGUILayout.IntField(graphComponent.m_maxTries, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("CA values", headerStyle);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Border Size", GUILayout.Width(100));
        graphComponent.m_borderSize = EditorGUILayout.IntField(graphComponent.m_borderSize, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Use Random", GUILayout.MaxWidth(105));
        EditorGUILayout.PropertyField(m_useRandom,GUIContent.none, GUILayout.MaxWidth(30));
        EditorGUILayout.LabelField("Random Fill Percent", GUILayout.MaxWidth(70));
        graphComponent.m_randomFillPercent = EditorGUILayout.IntSlider(graphComponent.m_randomFillPercent, 0, 100, GUILayout.MaxWidth(200));
        GUILayout.Space(30f);
        EditorGUILayout.LabelField("Smooth Iterations", GUILayout.MaxWidth(120));
        graphComponent.m_smoothIterations = EditorGUILayout.IntField(graphComponent.m_smoothIterations, GUILayout.MaxWidth(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("CA Spread", headerStyle);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Depth", GUILayout.MaxWidth(50));
        graphComponent.m_depth = EditorGUILayout.IntField(graphComponent.m_depth, GUILayout.MaxWidth(30));
        GUILayout.Space(30f);
        EditorGUILayout.LabelField("Random Node Depth Min", GUILayout.MaxWidth(150));
        graphComponent.m_randomNodeDepthMin = EditorGUILayout.IntField(graphComponent.m_randomNodeDepthMin, GUILayout.MaxWidth(30));
        GUILayout.Space(30f);
        EditorGUILayout.LabelField("Random Node Depth Max", GUILayout.MaxWidth(150));
        graphComponent.m_randomNodeDepthMax = EditorGUILayout.IntField(graphComponent.m_randomNodeDepthMax, GUILayout.MaxWidth(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pre-authored Roooms", headerStyle);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Room Sets", GUILayout.Width(100));
        graphComponent.m_roomSets = (PreAuthoredRoomSO)EditorGUILayout.ObjectField(graphComponent.m_roomSets, typeof(Class));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Difficulty Curves", headerStyle);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Apply Curve", GUILayout.MaxWidth(105));
        EditorGUILayout.PropertyField(m_applyCurve, GUIContent.none, GUILayout.MaxWidth(30));
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Apply Interval Value", GUILayout.MaxWidth(130));
        EditorGUILayout.PropertyField(m_applyIntervalValue, GUIContent.none, GUILayout.MaxWidth(30));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Difficulty Curve", GUILayout.Width(130));
        graphComponent.m_difficultyCurve = EditorGUILayout.CurveField(graphComponent.m_difficultyCurve);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Entity Spawn Data", headerStyle);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Use Poisson", GUILayout.MaxWidth(105));
        EditorGUILayout.PropertyField(m_usePoisson, GUIContent.none, GUILayout.MaxWidth(30));
        GUILayout.Space(30f);
        EditorGUILayout.LabelField("Use Jitter", GUILayout.MaxWidth(105));
        EditorGUILayout.PropertyField(m_useJitter, GUIContent.none, GUILayout.MaxWidth(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(15);
        if (GUILayout.Button("Run Rule."))
        {
            graphComponent.Reset();
            bool generated = graphComponent.Generate();

            if (generated)
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

        serializedObject.ApplyModifiedProperties();
    }
}
