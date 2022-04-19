using System;
using UnityEngine;

[System.Serializable]
public class Spawnpoint : ClickablePoint
{
    private MeshRenderer renderer;
    private Color originalColour;

    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        originalColour = renderer.material.color;
    }
    
    public void ResetToDefaultColour() => renderer.material.color = originalColour;

    public void ChangeToSelectionColour() => renderer.material.color = Color.red;
}