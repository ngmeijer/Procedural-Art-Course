using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public GameObject IntersectionPrefab;
    public GameObject RoadPrefab;

    public List<Node> allNodes;
    public bool faultDetected;

    public List<Node> faultyIntersections = new List<Node>();

    public void InitializeStreets(List<Node> pNodes)
    {
        allNodes = pNodes;

        CreateIntersections();
        CreateRoads();
    }

    public void CreateIntersections()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];
            Vector3 spawnPosition = new Vector3(currentNode.position.x, currentNode.position.y + 0.01f,
                currentNode.position.z);

            GameObject intersection =
                Instantiate(IntersectionPrefab, spawnPosition, Quaternion.identity, currentNode.gameObject.transform);

            if (i != allNodes.Count - 1) currentNode.transform.LookAt(allNodes[i + 1].transform.position);

            currentNode.intersectionRotation = intersection.transform.rotation;
            currentNode.InterSection = intersection;

            currentNode.UpdateVertexCoordinates();
        }

        faultyIntersections.Add(allNodes[1]);
    }

    public void CreateRoads()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];

            for (int j = 0; j < currentNode.connectedNodes.Count; j++)
            {
                createPlaneMesh(currentNode, currentNode.connectedNodes[j]);
            }
        }
    }

    private void createPlaneMesh(Node pMainNode, Node pConnectedNode)
    {
        Vector3 vertex1 = pMainNode.vertexCoordinates[0];
        Vector3 vertex2 = pMainNode.vertexCoordinates[1];
        Vector3 vertex3 = pConnectedNode.vertexCoordinates[2];
        Vector3 vertex4 = pConnectedNode.vertexCoordinates[3];

        GameObject go = new GameObject("Plane");
        MeshFilter meshFilter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer meshRenderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            vertex1,
            vertex2,
            vertex3,
            vertex4
        };

        Debug.Log(mesh.vertices[0]);
        
        mesh.uv = new Vector2[]
        {
            new Vector2(),
            new Vector2(),
            new Vector2(),
            new Vector2(),
        };

        mesh.triangles = new int[] {0, 1, 2, 0, 2, 3};

        meshFilter.mesh = mesh;
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];

            Vector3 labelPosition = new Vector3(currentNode.position.x, currentNode.position.y + 0.5f,
                currentNode.position.z);
            if (currentNode.connectedNodes.Count != 0)
                Handles.Label(labelPosition,
                    $"Node ID: {currentNode.name}\nDistance to {currentNode.connectedNodes[0]}: " +
                    currentNode.distanceToNextNode);
        }


        for (int i = 0; i < allNodes.Count; i++)
        {
        }
    }
}