using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : ClickablePoint
{
    public GameObject InterSection;
    public List<Node> connectedNodes = new List<Node>();
    public List<Node> alreadyConnectedNodes = new List<Node>();

    public Quaternion intersectionRotation;

    public float distanceToNextNode;

    public Vector3[] vertexCoordinates;
    public Vector3[] allVertices;

    public void UpdateVertexCoordinates()
    {
        allVertices = InterSection.GetComponent<MeshFilter>().sharedMesh.vertices;
        
        vertexCoordinates = new[]
        {
            transform.TransformPoint(allVertices[0]),
            transform.TransformPoint(allVertices[10]),
            transform.TransformPoint(allVertices[110]),
            transform.TransformPoint(allVertices[120]),
        };

        for (int i = 0; i < vertexCoordinates.Length; i++)
        {
            vertexCoordinates[i].y = 0.5f;
        }
    }
}