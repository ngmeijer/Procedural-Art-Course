using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private CityLayout currentSaveFile;

    private void createSaveFile(List<Node> pReceivedNodes)
    {
        CityLayout asset = ScriptableObject.CreateInstance<CityLayout>();
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