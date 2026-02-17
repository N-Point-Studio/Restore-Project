using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IClick, IDrag, IInspectable
{
    public Transform Transform => stateMachine.transform;

    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.ResetTransform();
        Debug.Log($"{stateMachine.name} Entered Idle State");
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void OnClick()
    {
        // Debug.Log($"{stateMachine.name} Click");
        stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
    }

    public void OnStart()
    {
        // Debug.Log($"{stateMachine.name} Start");
    }
    public void OnDragPerformed(Vector3 worldPos)
    {
        stateMachine.transform.position = worldPos;
        // Debug.Log($"{stateMachine.name} Drag");
    }
    public void OnEnd()
    {
        // Debug.Log($"{stateMachine.name} End");
    }

    public void EnterInspect() { stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine)); }
    public void ExitInspect() { }
}