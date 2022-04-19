using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
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
    private bool showBuildingEditor;
    private Vector3 buildingSize = new Vector3(5, 0, 5);
    private Vector3 buildingOffset = new Vector3(3,0,3);
    private float stackHeight = 1f;
    private float stackBuildDelay = 0.25f;
    private Vector2 stackHeightLimits = new Vector2(0, 10);
    private bool enableBillboards;
    private int selectedBuildingIndex;
    private int selectedCityBlockIndex;

    private UtilitySettings utilitySettings = new UtilitySettings()
    {
        HouseDistanceFactor = 3,
        HouseWeightFactor = 100,

        SkyScraperDistanceFactor = 11,
        SkyscraperWeightFactor = 45,
        
        MinRandomValue = -10,
        MaxRandomValue = 10
    };

    private int selectedStackIndex;

    private int selectedNewStackPrefabIndex;

    [MenuItem("Window/City Generator")]
    public static void ShowWindow()
    {
        CityGenerationWindow window = GetWindow<CityGenerationWindow>(false, "City Generator", true);
        window.Show();
    }

    private void OnEnable()
    {
        generator = FindObjectOfType<GeneratorFSM>();
        OnValidate();
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
        
        GUILayout.Space(20);
        handleNodeGUI();
        GUILayout.Space(40);
        handleRoadGUI();
        GUILayout.Space(40);
        handleCityBlockGUI();
        GUILayout.Space(40);
        handleBuildingSpecificGUI();
        
        if (GUI.changed) OnValidate();
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
        GUI.backgroundColor = Color.white;

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
        GUI.backgroundColor = Color.white;

        showCityBlockEditor = EditorGUILayout.Foldout(showCityBlockEditor, "City block editing", true, foldoutStyle);

        if (showCityBlockEditor)
        {
            GUILayout.Space(10);
            
            prepareCityBlockSettings();
            
            GUILayout.Space(40);

            prepareCityBlockButtons();
        }
    }

    private void prepareCityBlockButtons()
    {
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

        GUI.backgroundColor = Color.magenta;
        if (GUILayout.Button("Destroy index-selected \ncity block", buttonStyle, GUILayout.Height(50)))
            generator.ProcessCityBlockActionRequest(CityBlockActions.Destroy);

        GUILayout.EndHorizontal();
    }

    private void prepareCityBlockSettings()
    {
        GUILayout.Label("City Block settings", subHeaderStyle, GUILayout.Height(30));
        GUILayout.Label("These settings will be applied to all buildings in the current city block.", paragraphStyle, GUILayout.Height(30));

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
        EditorGUILayout.LabelField("Stack height", GUILayout.Width(120), GUILayout.Height(20));
        stackHeight = EditorGUILayout.Slider(stackHeight, 0, stackHeightLimits.y);
        EditorGUILayout.LabelField("Max:", GUILayout.Width(30));
        stackHeightLimits.y = EditorGUILayout.FloatField(stackHeightLimits.y, GUILayout.Width(45),GUILayout.Height(20));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stack build delay", GUILayout.Width(120), GUILayout.Height(20));
        stackBuildDelay = EditorGUILayout.Slider(stackBuildDelay, 0, 1.5f);
        GUILayout.EndHorizontal();

        GUILayout.Space(30);
        
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        
        EditorGUILayout.LabelField("House distance factor", GUILayout.Width(150), GUILayout.Height(20));
        utilitySettings.HouseDistanceFactor = EditorGUILayout.FloatField(utilitySettings.HouseDistanceFactor, GUILayout.Height(20));

        EditorGUILayout.LabelField("House weight factor", GUILayout.Width(120), GUILayout.Height(20));
        utilitySettings.HouseWeightFactor = EditorGUILayout.FloatField(utilitySettings.HouseWeightFactor, GUILayout.Height(20));
        
        GUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        GUILayout.BeginVertical();
        
        EditorGUILayout.LabelField("Skyscraper distance factor", GUILayout.Width(155), GUILayout.Height(20));
        utilitySettings.SkyScraperDistanceFactor = EditorGUILayout.FloatField(utilitySettings.SkyScraperDistanceFactor, GUILayout.Height(20));

        EditorGUILayout.LabelField("Skyscraper weight factor", GUILayout.Width(150), GUILayout.Height(20));
        utilitySettings.SkyscraperWeightFactor = EditorGUILayout.FloatField(utilitySettings.SkyscraperWeightFactor, GUILayout.Height(20));
        
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    
    private void handleBuildingSpecificGUI()
    {
        GUI.backgroundColor = Color.white;

        showBuildingEditor = EditorGUILayout.Foldout(showBuildingEditor, "Building editing", true, foldoutStyle);

        if (showBuildingEditor)
        {
            EditorGUILayout.LabelField("Selected city block index", GUILayout.Width(150), GUILayout.Height(20));
            selectedCityBlockIndex = EditorGUILayout.IntSlider(selectedCityBlockIndex, -1, generator.CityBlockCount - 1);
            
            EditorGUILayout.LabelField("Selected building index", GUILayout.Width(150), GUILayout.Height(20));
            selectedBuildingIndex = EditorGUILayout.IntSlider(selectedBuildingIndex, -1, generator.BuildingCount - 1);
            
            EditorGUILayout.LabelField("Selected stack index", GUILayout.Width(150), GUILayout.Height(20));
            selectedStackIndex = EditorGUILayout.IntSlider(selectedStackIndex, -1, generator.StackCount - 1);
            
            EditorGUILayout.LabelField("Selected new stack prefab index", GUILayout.Width(185), GUILayout.Height(20));
            selectedNewStackPrefabIndex = EditorGUILayout.IntSlider(selectedNewStackPrefabIndex, -1, generator.StackPrefabCount - 1);
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Regenerate building", buttonStyle, GUILayout.Height(50)))
            {
                generator.ProcessCityBlockActionRequest(CityBlockActions.RegenerateBuilding);
            }
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Confirm new stack", buttonStyle, GUILayout.Height(50)))
            {
                generator.ProcessCityBlockActionRequest(CityBlockActions.ReplaceStack);
            }
            GUILayout.EndHorizontal();
        }
    }

    private void OnValidate()
    {
        if (generator == null) generator = FindObjectOfType<GeneratorFSM>();
        
        generator.buildingSize = new Vector3(buildingSize.x, 0f, buildingSize.z);
        generator.buildingOffset = new Vector3(buildingOffset.x, 0f, buildingOffset.z);
        
        generator.stackHeight = stackHeight;
        generator.stackBuildDelay = stackBuildDelay;
        generator.selectedCityBlockIndex = selectedCityBlockIndex;
        generator.selectedBuildingIndex = selectedBuildingIndex;
        generator.utilitySettings = utilitySettings;
        generator.selectedStackIndex = selectedStackIndex;
        generator.selectedNewStackPrefabIndex = selectedNewStackPrefabIndex;
        
        generator.UpdateDependencies();
    }
}