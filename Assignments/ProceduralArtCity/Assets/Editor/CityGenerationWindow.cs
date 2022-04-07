using System;
using UnityEditor;
using UnityEngine;

public class CityGenerationWindow : EditorWindow
{
    private GeneratorFSM generator;
    private Node_EditModes currentEditMode;
    private Node_EditModes newEditMode = Node_EditModes.PlaceNode;
    private bool changedNodeEditMode;
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
    private float stackHeight;
    private Vector2 stackHeightLimits;

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
        
        GUILayout.Space(20);
        handleNodeGUI();
        GUILayout.Space(40);
        handleRoadGUI();
        GUILayout.Space(40);
        handleCityBlockGUI();

        if (GUI.changed) OnValidate();
    }

    private void handleGeneralGUI()
    {
    }

    private void handleNodeGUI()
    {
        showNodeEditor = EditorGUILayout.Foldout(showNodeEditor, "Node editing", true, foldoutStyle);

        if (showNodeEditor)
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Place", buttonStyle, GUILayout.Height(40)))
            {
                newEditMode = Node_EditModes.PlaceNode;
                changedNodeEditMode = true;
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove", buttonStyle, GUILayout.Height(40)))
            {
                newEditMode = Node_EditModes.RemoveNode;
                changedNodeEditMode = true;
            }

            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Move", buttonStyle, GUILayout.Height(40)))
            {
                newEditMode = Node_EditModes.MoveNode;
                changedNodeEditMode = true;
            }

            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("Connect", buttonStyle, GUILayout.Height(40)))
            {
                newEditMode = Node_EditModes.ConnectNode;
                changedNodeEditMode = true;
            }

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Disconnect", buttonStyle, GUILayout.Height(40)))
            {
                newEditMode = Node_EditModes.DisconnectNode;
                changedNodeEditMode = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.grey;
            if (GUILayout.Button("(Re)calculate spawnpoints", buttonStyle, GUILayout.Height(30)))
            {
                generator.ProcessSpawnpointRegenerationRequest();
            }

            if (GUILayout.Button("Confirm map", buttonStyle, GUILayout.Height(30)))
            {
                if (!NodeEditor.HasCalculatedSpawnpoints) return;
                generator.ProcessNewGenerationModeRequest(FSM_States.GenerateNodes);
            }

            GUILayout.EndHorizontal();
        }

        if (changedNodeEditMode)
        {
            generator.ProcessNewNodeEditModeRequest(newEditMode);
            currentEditMode = newEditMode;
            changedNodeEditMode = false;
        }
    }

    private void handleRoadGUI()
    {
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
        showCityBlockEditor = EditorGUILayout.Foldout(showCityBlockEditor, "City block editing", true, foldoutStyle);

        if (showCityBlockEditor)
        {
            GUILayout.Space(10);

            GUILayout.Label("Building settings", subHeaderStyle, GUILayout.Height(30));
            buildingType = (BuildingType) EditorGUILayout.EnumPopup("Building type:", buildingType);
            generator.ProcessCityBlockPreferredBuildingType(buildingType);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Building \nwidth", GUILayout.Width(50), GUILayout.Height(30));
            buildingSize.x = EditorGUILayout.FloatField(buildingSize.x, GUILayout.Height(30));

            EditorGUILayout.LabelField("Building \ndepth", GUILayout.Width(50), GUILayout.Height(30));
            buildingSize.z = EditorGUILayout.FloatField(buildingSize.z, GUILayout.Height(30));

            GUILayout.Space(50);

            EditorGUILayout.LabelField("Building \noffset X", GUILayout.Width(50), GUILayout.Height(30));
            buildingOffset.x = EditorGUILayout.FloatField(buildingOffset.x, GUILayout.Height(30));

            EditorGUILayout.LabelField("Building \noffset Z", GUILayout.Width(50), GUILayout.Height(30));
            buildingOffset.z = EditorGUILayout.FloatField(buildingOffset.z, GUILayout.Height(30));

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stack height", GUILayout.Width(80), GUILayout.Height(20));
            
            stackHeight = EditorGUILayout.Slider(stackHeight, 0, stackHeightLimits.y);
            
            EditorGUILayout.LabelField("Max:", GUILayout.Width(30));
            stackHeightLimits.y = EditorGUILayout.FloatField(stackHeightLimits.y, GUILayout.Width(45),GUILayout.Height(20));

            GUILayout.EndHorizontal();

            GUILayout.Space(40);
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

    private void OnValidate()
    {
        generator.buildingSize = new Vector3(buildingSize.x, 0f, buildingSize.z);
        generator.buildingOffset = new Vector3(buildingOffset.x, 0f, buildingOffset.z);

        generator.stackHeight = stackHeight;
    }
}