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

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Node editing modes");

        if (GUILayout.Button("Place")) newEditMode = NodeEditModes.PlaceNode;
        if (GUILayout.Button("Remove")) newEditMode = NodeEditModes.RemoveNode;
        if (GUILayout.Button("Move")) newEditMode = NodeEditModes.MoveNode;
        if (GUILayout.Button("Connect")) newEditMode = NodeEditModes.ConnectNode;
        if (GUILayout.Button("Disconnect")) newEditMode = NodeEditModes.DisconnectNode;
        if (GUILayout.Button("Confirm map")) editorTarget.onModeExit.Invoke(FSM_States.GenerateNodes);

        if (newEditMode != editorTarget.CurrentMode)
        {
            editorTarget.onSelectNewMode.Invoke(newEditMode);
            editorTarget.CurrentMode = newEditMode;
        }
    }
}