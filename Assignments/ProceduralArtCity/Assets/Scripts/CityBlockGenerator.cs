using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CityBlockGenerator : MonoBehaviour
{
    [SerializeField] private List<List<Vector3>> cityBlocksData = new List<List<Vector3>>();
    
    private void Start()
    {
        NodeSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        UIManager.onCityBlockInitialize.AddListener(createNewCityBlockList);
    }

    private void createNewCityBlockList()
    {
        cityBlocksData.Add(new List<Vector3>());
    }

    private void addNodeToCityBlockCorners(Node pNode, Vector3 pMousePosition)
    {
        if(pNode == null) return;
        
        Debug.Log($"Selected {pNode.name} for city block");
    }
}
