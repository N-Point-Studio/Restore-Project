using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IDragObject, IInspectable
{
    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.ResetTransform();
        stateMachine.state = ArtefactPieceState.Idle;
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public Transform GetTransform() => stateMachine.transform;
    public void EnterInspect(Transform targetTransform)
    {
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, targetTransform, ArtefactPieceState.Inspect));
    }
    public void ExitInspect() { }

    public void OnDragStarted(Vector3 worldPos)
    {
        stateMachine.transform.position = worldPos;
    }

    public void OnDragPerformed(Vector3 worldPos)
    {
        stateMachine.transform.position = worldPos;
    }

    public void OnDragEnded(Vector3 worldPos)
    {
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
}