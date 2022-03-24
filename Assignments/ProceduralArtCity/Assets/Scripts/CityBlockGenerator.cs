using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CityBlock
{
    public List<Vector3> outerCorners = new List<Vector3>();
}

public class CityBlockGenerator : MonoBehaviour
{
    [SerializeField] private List<CityBlock> cityBlocksData = new List<CityBlock>();
    private int currentSelectedIndex = -1;
    private Vector3 currentCentroidPoint;
    private bool generatorActive = false;
    
    private void Start()
    {
        UIManager.onClickNewMode.AddListener(checkMode);
        NodeSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        UIManager.onCityBlockInitialize.AddListener(createNewCityBlockList);
        UIManager.onCityBlockFinish.AddListener(finishCityBlock);
    }

    private void checkMode(string pNewMode)
    {
        if (pNewMode != "CityBlockEditMode") return;
        
        generatorActive = true;
    }

    private void createNewCityBlockList()
    {
        CityBlock cityBlock = new CityBlock();
        cityBlocksData.Add(cityBlock);
        currentSelectedIndex += 1;
    }

    private void addNodeToCityBlockCorners(Node pNode, Vector3 pMousePosition)
    {
        if (!generatorActive) return;
        if (pNode == null) return;

        CityBlock currentCityBlock = cityBlocksData[currentSelectedIndex];
        
        if (!currentCityBlock.outerCorners.Contains(pNode.position)) currentCityBlock.outerCorners.Add(pNode.position);
    }

    private void finishCityBlock()
    {
        //Calculate centroid
        //Calculate inner corners based on centroid, outer corners and 
        List<Vector3> outerCorners = cityBlocksData[currentSelectedIndex].outerCorners;

        Vector3 centroid = new Vector3();
        centroid.y = 0.5f;

        float sumX = 0;
        float sumY = 0;
        for (int i = 0; i < outerCorners.Count; i++)
        {
            sumX += outerCorners[i].x;
            sumY += outerCorners[i].y;
        }

        centroid.x = sumX / outerCorners.Count;
        centroid.y = sumY / outerCorners.Count;
        
        Debug.Log(centroid.ToString("F7"));

        currentCentroidPoint = centroid;
    }

    private void OnDrawGizmos()
    {
        if (currentCentroidPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(currentCentroidPoint, 2f);
    }
}