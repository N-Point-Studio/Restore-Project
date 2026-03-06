using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IClick, IDragObject, IInspectable, IAssembled
{
    public string PieceId => stateMachine.pieceId;
    public bool IsInspected => stateMachine.isInspected;
    public List<ConnectionSocket> GetSockets() => stateMachine.sockets;

    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.ResetTransform();
        stateMachine.state = ArtefactPieceState.Idle;
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void OnInteractStart() { stateMachine.ResetTransform(); }
    public void OnInteractEnd() { stateMachine.ResetTransform(); }

    public void OnClick() { }
    // public void OnDragPerformed(Vector3 worldPos) { stateMachine.transform.position = worldPos; }

    public void EnterInspect(Transform targetTransform)
    {
        stateMachine.isInspected = true;
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, targetTransform, ArtefactPieceState.Inspect));
    }
    public void ExitInspect() { }

    public void OnAssembled(IAssembled parent, Transform transform)
    {
        Debug.Log($"Onassemble di {stateMachine.name} ke {transform.position}");
        stateMachine.parent = parent;
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, transform, ArtefactPieceState.Assembled));
    }
    public void OnDetached() { }
    public Transform GetTransform() => stateMachine.transform;
    public IAssembled GetAssembleParrent() => stateMachine.parent;
    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    public void ReleaseSocketWith(string otherId) { }

    public void OnDragStarted(Vector3 worldPos)
    {
        Debug.Log("Drag started");
        stateMachine.transform.position = worldPos;
    }

    public void OnDragPerformed(Vector3 worldPos)
    {
        Debug.Log("Drag performed");

        stateMachine.transform.position = worldPos;
    }

    public void OnDragEnded(Vector3 worldPos)
    {
        Debug.Log("Drag ended");

        stateMachine.transform.position = worldPos;
        stateMachine.ResetTransform();
    }
}