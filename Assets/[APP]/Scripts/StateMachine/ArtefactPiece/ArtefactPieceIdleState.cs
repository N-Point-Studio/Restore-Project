using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IClick, IDrag, IInspectable, IAssembled
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

    public void OnInteractStart() { stateMachine.ResetTransform(); }
    public void OnInteractEnd() { stateMachine.ResetTransform(); }

    public void OnClick() { }
    public void OnDragPerformed(Vector3 worldPos) { stateMachine.transform.position = worldPos; }

    public void EnterInspect(Vector3 targetPosition) { stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, targetPosition)); }
    public void ExitInspect() { }

    public void OnAssembled(IAssembled parent)
    {
        stateMachine.parent = parent;
        stateMachine.SwitchState(new ArtefactPieceAssembledState(stateMachine));
    }
    public void OnDetached() { }
    public Transform GetTransform() => stateMachine.transform;
    public IAssembled GetAssembleParrent() => stateMachine.parent;
    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    public void ReleaseSocketWith(string otherId) { }
}