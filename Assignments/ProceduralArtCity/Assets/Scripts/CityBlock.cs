using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CityBlock
{
    public List<Vector3> outerCorners = new List<Vector3>();
    public Transform parent;
    public Vector3 centroid;
    public Mesh spawnAreaMesh;
    public List<Spawnpoint> spawnPoints = new List<Spawnpoint>();
    public List<ProceduralBuilding> spawnedBuildings = new List<ProceduralBuilding>();
}