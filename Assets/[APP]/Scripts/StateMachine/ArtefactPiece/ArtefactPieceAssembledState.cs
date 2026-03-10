using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IAssembled, IInspectable
{
    public Transform GetTransform() => stateMachine.transform;
    public IAssembled GetAssembleParrent() => stateMachine.parent;
    public string PieceId => stateMachine.pieceId;
    public bool IsInspected => stateMachine.isInspected;
    public List<ConnectionSocket> GetSockets() => stateMachine.sockets;

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

    public void OnAssembled(IAssembled parent, Transform transform)
    {
        stateMachine.parent = parent;
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, transform, ArtefactPieceState.Assembled));
    }
    public void OnDetached()
    {
        stateMachine.parent = null;
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    public void ReleaseSocketWith(string otherId) { }
}
