using UnityEngine;
using UnityEngine.Events;

public abstract class FSM_State : MonoBehaviour
{
    public bool isActive;

    public static bool DisableGizmos;
    
    public abstract void EnterState();
    public abstract void ExitState();
}