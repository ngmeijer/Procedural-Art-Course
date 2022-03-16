
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public List<Node> allNodes;

    public void GenerateRoads(List<Node> pNodes)
    {
        Debug.Log("Generating roads...");
        allNodes = pNodes;
    }
}