using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IHold, IInspectable, IAssembled
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

    public void OnInteractStart() { }
    public void OnHoldPerformed() { }
    public void OnInteractEnd() { }

    public void EnterInspect(Vector3 targetPosition) { }
    public void ExitInspect() { stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine)); }

    public void OnAssembled(IAssembled parent) { }
    public void OnDetached() { stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine)); }
    public IAssembled GetAssembleParrent() => stateMachine.parent;

    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    public void ReleaseSocketWith(string otherId) { }
}