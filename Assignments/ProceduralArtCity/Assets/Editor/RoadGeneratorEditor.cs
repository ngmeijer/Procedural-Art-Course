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

        if (GUILayout.Button("Create roads"))
        {
            editorTarget.InitializeRoads();
        };
    }
}