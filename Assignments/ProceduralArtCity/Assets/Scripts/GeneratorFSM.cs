
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum NodeEditModes
{
     PlaceNode,
     RemoveNode,
     MoveNode,
     ConnectNode,
     DisconnectNode,
     NoneSelected,
}

public enum FSM_States
{
     GenerateNodes,
     GenerateRoads,
     GenerateCityBlocks
}


public class GeneratorFSM : MonoBehaviour
{
     public static UnityEvent<NodeEditModes> broadcastNodeEditModeChange = new UnityEvent<NodeEditModes>();
     private NodeEditor nodeEditor;
     private RoadGenerator roadGenerator;
     private CityBlockGenerator cityBlockGenerator;
     private FSM_State currentState;
     
     private void OnEnable()
     {
          nodeEditor = FindObjectOfType<NodeEditor>();
          roadGenerator = FindObjectOfType<RoadGenerator>();
          cityBlockGenerator = FindObjectOfType<CityBlockGenerator>();
          
          nodeEditor.onSelectNewMode.AddListener(listenToNewNodeEditMode);
          
          nodeEditor.onModeExit.AddListener(listenToNewGenerationMode);
          roadGenerator.onModeExit.AddListener(listenToNewGenerationMode);
          cityBlockGenerator.onModeExit.AddListener(listenToNewGenerationMode);
          currentState = nodeEditor;
     }

     private void listenToNewNodeEditMode(NodeEditModes pNewMode)
     {
          broadcastNodeEditModeChange.Invoke(pNewMode);
     }

     private void listenToNewGenerationMode(FSM_States pOldState)
     {
          switch (pOldState)
          {
               case FSM_States.GenerateNodes:
                    currentState.ExitState();
                    currentState = roadGenerator;
                    break;
               case FSM_States.GenerateRoads:
                    currentState.ExitState();
                    currentState = cityBlockGenerator;
                    break;
               case FSM_States.GenerateCityBlocks:
                    currentState.ExitState();
                    break;
          }
     }
}
