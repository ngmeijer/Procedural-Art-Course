using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Test", menuName = "ScriptableObjects/SavedNodes")]
public class Nodes_SO : ScriptableObject
{
    public List<Node> Nodes = new List<Node>();
}
