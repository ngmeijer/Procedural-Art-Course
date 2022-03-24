using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CityBlock
{
    public List<Vector3> outerCorners = new List<Vector3>();
    public List<Vector3> innerCorners = new List<Vector3>();
    public Vector3 centroid = new Vector3();
    public Mesh spawnAreaMesh;
}