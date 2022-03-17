using UnityEditor;
using UnityEngine;

[CustomEditor((typeof(Node)))]
public class TransformNodeEditor : Editor
{
    public void OnSceneGUI()
    {
        Node editorTarget = (target as Node);

        EditorGUI.BeginChangeCheck();
        Quaternion rot = Handles.RotationHandle(editorTarget.intersectionRotation, Vector3.zero);
        Debug.Log("showing rotation handle");
        if (EditorGUI.EndChangeCheck())
        {
            //Undo.RecordObject(target, "Rotated RotateAt Point");
            editorTarget.intersectionRotation = rot;
            editorTarget.Update();
        }
    }
}