using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(NodeEditor))]
public class ManageNodesEditor : Editor
{
    private NodeEditor editorTarget;
    private NodeEditModes newEditMode;

    private void OnEnable()
    {
        editorTarget = (NodeEditor) target;
        editorTarget.CurrentMode = NodeEditModes.NoneSelected;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        float inspectorWidth = EditorGUIUtility.currentViewWidth;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Node editing modes");

        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15
        };

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Place", style)) newEditMode = NodeEditModes.PlaceNode;

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove", style))
            newEditMode = NodeEditModes.RemoveNode;

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Move", style)) newEditMode = NodeEditModes.MoveNode;

        GUI.backgroundColor = Color.magenta;
        if (GUILayout.Button("Connect", style)) newEditMode = NodeEditModes.ConnectNode;

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Disconnect", style)) newEditMode = NodeEditModes.DisconnectNode;
        
        EditorGUILayout.Space(20);
        GUI.backgroundColor = Color.gray;
        if (GUILayout.Button("Recalculate spawnpoints", style)) editorTarget.RecalculateSpawnpoints();
        
        GUI.backgroundColor = Color.grey;
        if (GUILayout.Button("Confirm map", style))
        {
            if (!editorTarget.HasCalculatedSpawnpoints) return;
            editorTarget.onModeExit.Invoke(FSM_States.GenerateNodes);
        }

        if (newEditMode != editorTarget.CurrentMode)
        {
            editorTarget.onSelectNewMode.Invoke(newEditMode);
            editorTarget.CurrentMode = newEditMode;
        }
    }
}