using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralBuilding))]
public class ProceduralBuildingEditor : Editor
{
    private ProceduralBuilding editorTarget;

    private void OnEnable()
    {
        editorTarget = (ProceduralBuilding) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15
        };

        GUILayout.Space(20);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Regenerate building", style, GUILayout.Height(50)))
        {
            editorTarget.RegenerateBuilding();
        }
        
    }
}