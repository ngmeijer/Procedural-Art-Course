using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Test", menuName = "ScriptableObjects/CityLayout")]
public class CityLayout : ScriptableObject
{
    public List<Node> Nodes = new List<Node>();
    public List<Vector3> outerCorners = new List<Vector3>();
}
