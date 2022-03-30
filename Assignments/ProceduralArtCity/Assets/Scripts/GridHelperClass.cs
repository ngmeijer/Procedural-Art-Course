using System.Collections.Generic;
using UnityEngine;

public static class GridHelperClass
{
    public static Vector3 GetCentroidOfArea(List<Vector3> pPositions)
    {
        Vector3 centroid = new Vector3 {y = 0.5f};

        float sumX = 0;
        float sumZ = 0;
        for (int i = 0; i < pPositions.Count; i++)
        {
            sumX += pPositions[i].x;
            sumZ += pPositions[i].z;
        }

        centroid.x = sumX / pPositions.Count;
        centroid.z = sumZ / pPositions.Count;
        
        return centroid;
    }
}