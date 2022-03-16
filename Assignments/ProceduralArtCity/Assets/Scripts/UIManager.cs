using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    private GUIStyle labelStyle = new GUIStyle();
    public string currentMode = "none selected";
    private string modeInstructions;

    public UnityEvent onClickCreateSaveFile = new UnityEvent();
    public UnityEvent onClickSaveToFile = new UnityEvent();
    
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    private void Start()
    {
        labelStyle.fontSize = 30;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 50), "Mode:", labelStyle);
        GUI.Label(new Rect(100, 10, 100, 50), currentMode, labelStyle);
        GUI.Label(new Rect(10, 360, 100, 50), modeInstructions, labelStyle);
        GUI.Label(new Rect(1960, 150, 100, 50), "Press TAB to lock/unlock \ncamera movement", labelStyle);

        if (GUI.Button(new Rect(10, 70, 150, 50), "Place node"))
        {
            currentMode = "Place";
            modeInstructions = "Click somewhere to place a node.";
        }

        if (GUI.Button(new Rect(10, 130, 150, 50), "Remove node"))
        {
            currentMode = "Remove";
            modeInstructions = "Click node to delete.";
        }

        if (GUI.Button(new Rect(10, 190, 150, 50), "Move node"))
        {
            currentMode = "Move";
            modeInstructions = "Click a node to select it. \nUse mouse to move node. \nClick again to release.";
        }

        if (GUI.Button(new Rect(10, 250, 150, 50), "Connect node"))
        {
            currentMode = "Connect";
            modeInstructions = "Click another node to connect";
        }

        if (GUI.Button(new Rect(10, 310, 150, 50), "Disconnect node"))
        {
            currentMode = "Disconnect";
            modeInstructions = "Click one of the connected \nnodes to disconnect";
        }
        
        if (GUI.Button(new Rect(2100, 10, 150, 50), "Create new save file"))
        {
            onClickCreateSaveFile.Invoke();
        }
        
        if (GUI.Button(new Rect(2100, 70, 150, 80), "Save to currently \nselected file"))
        {
            onClickSaveToFile.Invoke();
        }
    }
}