using DG.Tweening;
using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IHold, IAssembled, IInspectable
{
    public Transform GetTransform() => stateMachine.transform;
    public IAssembled GetAssembleParrent() => stateMachine.parent;
    public string PieceId => stateMachine.pieceId;

    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Assembled;
        // stateMachine.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 5, 1f);
        stateMachine.transform.DOPunchRotation(new Vector3(5, 5, 0), 0.4f);
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void OnInteractStart() { }
    public void OnInteractEnd() { }
    public void OnHoldPerformed() { }

    public void EnterInspect(Transform targetPosition)
    {
        // stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
        stateMachine.SwitchState(new ArtefactPieceMoveState(stateMachine, targetPosition, ArtefactPieceState.Inspect));
    }
    public void ExitInspect() { }

    public void OnAssembled(IAssembled parent, Transform transform) { stateMachine.parent = parent; }
    public void OnDetached()
    {
        stateMachine.parent = null;
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);
    public void ReleaseSocketWith(string otherId) { }
}
