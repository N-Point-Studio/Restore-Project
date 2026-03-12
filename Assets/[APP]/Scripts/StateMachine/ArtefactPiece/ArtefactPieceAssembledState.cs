using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IInspectable
{
    public Transform GetTransform() => stateMachine.transform;
    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Assembled;
        stateMachine.transform.DOPunchRotation(new Vector3(5, 5, 0), 0.4f);
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void EnterInspect(Transform targetPosition)
    {
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, targetPosition, ArtefactPieceState.Inspect));
    }
    public void ExitInspect() { }
}
