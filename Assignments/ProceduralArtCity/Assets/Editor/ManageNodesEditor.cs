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
    private Node_EditModes newEditMode;

    private void OnEnable()
    {
        editorTarget = (NodeEditor) target;
        //editorTarget.CurrentMode = Node_EditModes.NoneSelected;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Node editing modes");

        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15
        };
        
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Place", style, GUILayout.Height(40))) newEditMode = Node_EditModes.PlaceNode;

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove", style, GUILayout.Height(40)))
            newEditMode = Node_EditModes.RemoveNode;

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Move", style, GUILayout.Height(40))) newEditMode = Node_EditModes.MoveNode;

        GUI.backgroundColor = Color.magenta;
        if (GUILayout.Button("Connect", style,GUILayout.Height(40))) newEditMode = Node_EditModes.ConnectNode;

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Disconnect", style, GUILayout.Height(40))) newEditMode = Node_EditModes.DisconnectNode;
        
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(20);
        GUI.backgroundColor = Color.gray;
        if (GUILayout.Button("Recalculate spawnpoints", style)) editorTarget.RecalculateSpawnpoints();
        
        GUI.backgroundColor = Color.grey;
        // if (GUILayout.Button("Confirm map", style))
        // {
        //     if (!editorTarget.HasCalculatedSpawnpoints) return;
        //     editorTarget.onModeExit.Invoke(FSM_States.GenerateNodes);
        // }
        //
        // if (newEditMode != editorTarget.CurrentMode)
        // {
        //     editorTarget.onSelectNewMode.Invoke(newEditMode);
        //     editorTarget.CurrentMode = newEditMode;
        // }
    }
}