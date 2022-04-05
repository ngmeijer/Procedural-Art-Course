using System;
using UnityEditor;
using UnityEngine;

public class CityGenerationWindow : EditorWindow
{
    private GeneratorFSM generator;
    private Node_EditModes newEditMode;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;

    [MenuItem("Window/City Generator")]
    public static void ShowWindow()
    {
        CityGenerationWindow window = GetWindow<CityGenerationWindow>(false, "City Generator", true);
        window.Show();
    }

    private void OnEnable()
    {
        generator = FindObjectOfType<GeneratorFSM>();
    }

    private void createGUIStyles()
    {
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
        };
    }

    private void OnGUI()
    {
        createGUIStyles();

        handleGeneralGUI();
        handleNodeGUI();
        handleRoadGUI();
        handleCityBlockGUI();
    }

    private void handleGeneralGUI()
    {
        
    }

    private void handleNodeGUI()
    {
        GUILayout.Label("Node Generation", headerStyle, GUILayout.Height(40));
        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Place", buttonStyle, GUILayout.Height(40))) newEditMode = Node_EditModes.PlaceNode;

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove", buttonStyle, GUILayout.Height(40)))
            newEditMode = Node_EditModes.RemoveNode;

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Move", buttonStyle, GUILayout.Height(40))) newEditMode = Node_EditModes.MoveNode;

        GUI.backgroundColor = Color.magenta;
        if (GUILayout.Button("Connect", buttonStyle, GUILayout.Height(40))) newEditMode = Node_EditModes.ConnectNode;

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Disconnect", buttonStyle, GUILayout.Height(40)))
            newEditMode = Node_EditModes.DisconnectNode;
        GUILayout.EndHorizontal();


        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.grey;
        if (GUILayout.Button("Recalculate spawnpoints", buttonStyle, GUILayout.Height(30)))
            generator.ProcessSpawnpointRegenerationRequest();
        if (GUILayout.Button("Confirm map", buttonStyle, GUILayout.Height(30)))
        {
            if (!NodeEditor.HasCalculatedSpawnpoints) return;
            generator.ProcessNewGenerationModeRequest(FSM_States.GenerateNodes);
        }

        GUILayout.EndHorizontal();

        if (newEditMode != NodeEditor.CurrentMode)
        {
            generator.ProcessNewNodeEditModeRequest(newEditMode);
        }
    }

    private void handleRoadGUI()
    {
        GUILayout.Space(20);
        GUILayout.Label("Road Generation", headerStyle, GUILayout.Height(40));
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create roads", buttonStyle, GUILayout.Height(50)))
            generator.ProcessRoadGenerationRequest();
    }

    private void handleCityBlockGUI()
    {
        GUILayout.Space(20);
        GUILayout.Label("City Block Generation", headerStyle, GUILayout.Height(40));

        Vector2 test = Vector2.zero;
        EditorGUILayout.Vector3Field("Building Size", test);
        EditorGUILayout.Vector3Field("Building Offset", test);

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.grey;
        if (GUILayout.Button("Create new \ncity block", buttonStyle, GUILayout.Height(50)))
            generator.ProcessCityBlockActionRequest(CityBlockActions.Create);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Finish current \ncity block", buttonStyle, GUILayout.Height(50)))
            generator.ProcessCityBlockActionRequest(CityBlockActions.Create);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Discard current \ncity block", buttonStyle, GUILayout.Height(50)))
            generator.ProcessCityBlockActionRequest(CityBlockActions.Create);

        GUILayout.EndHorizontal();
    }
}