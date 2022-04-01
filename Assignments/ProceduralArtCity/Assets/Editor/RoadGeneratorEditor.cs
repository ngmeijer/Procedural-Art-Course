using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadGenerator))]
public class RoadGeneratorEditor : Editor
{
    private RoadGenerator editorTarget;

    private void OnEnable()
    {
        editorTarget = (RoadGenerator) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15
        };

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create roads", style, GUILayout.Height(50))) editorTarget.InitializeRoads();
    }
}