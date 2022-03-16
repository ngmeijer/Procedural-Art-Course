using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private Nodes_SO currentSaveFile;
    
    private void Start()
    {
        NodeManager.Instance.eventTransferToNewFile.AddListener(createSaveFile);
        NodeManager.Instance.eventTransferToExistingFile.AddListener(saveToCurrentFile);
    }

    private void createSaveFile(List<Node> pReceivedNodes)
    {
        Nodes_SO asset = ScriptableObject.CreateInstance<Nodes_SO>();
        asset.Nodes = pReceivedNodes;
        
        AssetDatabase.CreateAsset(asset, "Assets/Scriptable Objects/SaveFile.asset");
        AssetDatabase.SaveAssets();
        
        EditorUtility.SetDirty(asset);
    }

    private void saveToCurrentFile(List<Node> pReceivedNodes)
    {
        
        currentSaveFile.Nodes.Add(pReceivedNodes[0]);
        
        EditorUtility.SetDirty(currentSaveFile);
    }
}