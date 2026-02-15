using UnityEngine;

public interface IInspectable
{
    void EnterInspect(); // Di sini StateMachine akan SwitchState ke InspectState
    void ExitInspect();  // Di sini StateMachine akan SwitchState ke IdleState
    Transform Transform { get; }
}