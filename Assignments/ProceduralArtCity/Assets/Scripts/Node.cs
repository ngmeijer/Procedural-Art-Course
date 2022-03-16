using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
    public Vector3 thisNodePosition;
    public List<Node> connectedNodes = new List<Node>();
}
