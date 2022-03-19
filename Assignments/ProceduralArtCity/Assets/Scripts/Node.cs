using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
    public Vector3 position;
    public GameObject InterSection;
    public List<Node> connectedNodes = new List<Node>();
    public List<Node> alreadyConnectedNodes = new List<Node>();

    public Quaternion intersectionRotation;

    public float distanceToNextNode;

    public Vector3[] vertexCoordinates;

    public void UpdateVertexCoordinates()
    {
        Vector3[] vertices = InterSection.GetComponent<MeshFilter>().sharedMesh.vertices;
        
        vertexCoordinates = new[]
        {
            transform.TransformPoint(vertices[0]),
            transform.TransformPoint(vertices[10]),
            transform.TransformPoint(vertices[110]),
            transform.TransformPoint(vertices[120]),
        };

        for (int i = 0; i < vertexCoordinates.Length; i++)
        {
            vertexCoordinates[i].y = 0.5f;
        }
    }
}
