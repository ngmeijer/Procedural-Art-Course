using System;
using UnityEditor;
using UnityEngine;

public class CityGenerationWindow : EditorWindow
{
    private GeneratorFSM generator;
    private Node_EditModes currentEditMode;
    private Node_EditModes newEditMode;
    private BuildingType buildingType;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle paragraphStyle;
    private GUIStyle foldoutStyle;
    private BuildingType preferredBuildingType;
    private bool showNodeEditor;
    private bool showRoadEditor;
    private bool showCityBlockEditor;
    private Vector3 buildingSize = new Vector3(5, 0, 0);
    private Vector3 buildingOffset;

    [MenuItem("Window/City Generator")]
    public static void ShowWindow()
    {
        CityGenerationWindow window = GetWindow<CityGenerationWindow>(false, "City Generator", true);
        window.Show();
    }

    private void OnEnable()
    {
        generator = FindObjectOfType<GeneratorFSM>();
        currentEditMode = generator.currentEditMode;
    }

    private void createGUIStyles()
    {
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25
        };

        subHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
        };

        paragraphStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
        };

        foldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontSize = 20,
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
        GUILayout.Space(20);
        showNodeEditor = EditorGUILayout.Foldout(showNodeEditor, "Node editing", true, foldoutStyle);

        if (showNodeEditor)
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Place", buttonStyle, GUILayout.Height(40))) newEditMode = Node_EditModes.PlaceNode;

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove", buttonStyle, GUILayout.Height(40)))
                newEditMode = Node_EditModes.RemoveNode;

            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Move", buttonStyle, GUILayout.Height(40))) newEditMode = Node_EditModes.MoveNode;

            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("Connect", buttonStyle, GUILayout.Height(40)))
                newEditMode = Node_EditModes.ConnectNode;

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
        }
        
        if (newEditMode != currentEditMode)
        {
            Debug.Log(newEditMode);
            generator.ProcessNewNodeEditModeRequest(newEditMode);
            currentEditMode = newEditMode;
        }
    }

    private void handleRoadGUI()
    {
        GUILayout.Space(20);
        showRoadEditor = EditorGUILayout.Foldout(showRoadEditor, "Road editing", true, foldoutStyle);

        if (showRoadEditor)
        {
            GUILayout.Space(10);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Create roads", buttonStyle, GUILayout.Height(50)))
                generator.ProcessRoadGenerationRequest();
        }
    }

    private void handleCityBlockGUI()
    {
        GUILayout.Space(20);
        showCityBlockEditor = EditorGUILayout.Foldout(showCityBlockEditor, "City block editing", true, foldoutStyle);

        if (showCityBlockEditor)
        {
            GUILayout.Space(10);
            GUILayout.Label("Optional Parameters", subHeaderStyle, GUILayout.Height(30));
            GUILayout.Label(
                "Only edit these options if you want the neighbourhood \nto have a specific feeling/building type!",
                paragraphStyle, GUILayout.Height(40));

            GUILayout.Space(5);
            buildingType = (BuildingType) EditorGUILayout.EnumPopup("Building type:", buildingType);
            generator.ProcessCityBlockPreferredBuildingType(buildingType);
            GUILayout.Space(10);

            GUILayout.Label("Mandatory Parameters", subHeaderStyle, GUILayout.Height(30));
            GUILayout.BeginHorizontal();

            buildingSize = EditorGUILayout.Vector3Field("Building Size", buildingSize);
            buildingSize = EditorGUILayout.Vector3Field("Building Offset", buildingOffset);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey;
            if (GUILayout.Button("Create new \ncity block", buttonStyle, GUILayout.Height(50)))
                generator.ProcessCityBlockActionRequest(CityBlockActions.Create);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Finish current \ncity block", buttonStyle, GUILayout.Height(50)))
                generator.ProcessCityBlockActionRequest(CityBlockActions.Finish);

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Discard current \ncity block", buttonStyle, GUILayout.Height(50)))
                generator.ProcessCityBlockActionRequest(CityBlockActions.Discard);

            GUILayout.EndHorizontal();
        }
    }
}