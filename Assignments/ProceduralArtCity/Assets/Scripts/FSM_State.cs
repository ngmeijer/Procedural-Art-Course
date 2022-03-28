using UnityEngine;
using UnityEngine.Events;

public abstract class FSM_State : MonoBehaviour
{
    public bool isActive;

    public UnityEvent<FSM_States> onModeExit;
    
    public abstract void EnterState();
    public abstract void ExitState();
}