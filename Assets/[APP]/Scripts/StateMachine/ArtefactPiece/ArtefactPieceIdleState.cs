using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IDragObject, IArtefactPart
{
    public string PieceId => stateMachine.pieceId;

    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.ResetTransform();
        stateMachine.state = ArtefactPieceState.Idle;
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    //=== IDragObject ===
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

    //=== IAssemble

    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    public void ReleaseSocketWith(string otherId) { }
    public ArtefactPieceState CurrentState => stateMachine.state;
    public void OnAssembled(Transform targetTransform)
    {
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, targetTransform));
        AssembleEvents.OnAssemblePerformed?.Invoke();
    }
    public void OnDetached() { }

    public List<ConnectionSocket> GetSockets() => stateMachine.sockets;

    public Transform GetTransform() => stateMachine.transform;

    public void CorrectRotation(Quaternion rotation) => stateMachine.CorrectRotation(rotation);
}