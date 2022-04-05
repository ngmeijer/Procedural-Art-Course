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
        
        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15
        };
        
        GUILayout.BeginHorizontal();
        GUILayout.Space(50);

        GUI.backgroundColor = Color.grey;
        if (GUILayout.Button("Create new \ncity block", style, GUILayout.Height(50))) editorTarget.CreateEmptyCityBlock();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Finish current \ncity block", style, GUILayout.Height(50))) editorTarget.FinishCityBlock();
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Discard current \ncity block", style, GUILayout.Height(50))) editorTarget.DiscardCurrentCityBlock();
        
        GUILayout.EndHorizontal();
    }
}