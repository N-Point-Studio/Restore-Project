using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IInspectable
{
    public string PieceId => stateMachine.pieceId;
    public Transform GetTransform() => stateMachine.transform;
    public ArtefactPieceInspectState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Inspect;
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void EnterInspect(Transform targetPosition) { }
    public void ExitInspect()
    {
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
}