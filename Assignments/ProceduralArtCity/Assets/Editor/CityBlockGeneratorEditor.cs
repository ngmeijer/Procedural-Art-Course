using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityBlockGenerator))]
public class CityBlockGeneratorEditor : Editor
{
    private CityBlockGenerator editorTarget;

    private void OnEnable()
    {
        editorTarget = (CityBlockGenerator) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Create new city block")) editorTarget.CreateEmptyCityBlock();
        if (GUILayout.Button("Finish current city block")) editorTarget.FinishCityBlock();
        if (GUILayout.Button("Discard current city block")) editorTarget.DiscardCurrentCityBlock();
    }
}