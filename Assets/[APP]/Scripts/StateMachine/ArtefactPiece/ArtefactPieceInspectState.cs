using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IInspectable
{
    public string PieceId => stateMachine.pieceId;
    public Transform GetTransform() => stateMachine.transform;
    // public bool IsInspected => stateMachine.isInspected;
    // public List<ConnectionSocket> GetSockets() => stateMachine.sockets;

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
        // stateMachine.isInspected = false;

        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }

    // public void OnAssembled(IAssembled parent, Transform transform) { }
    // public void OnDetached() { stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine)); }
    // public IAssembled GetAssembleParrent() => stateMachine.parent;

    // public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    // public void ReleaseSocketWith(string otherId) { }
}