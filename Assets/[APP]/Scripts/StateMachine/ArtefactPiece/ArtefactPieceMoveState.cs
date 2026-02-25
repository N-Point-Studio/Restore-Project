using UnityEngine;
using DG.Tweening;

public class ArtefactPieceMoveState : ArtefactPieceBaseState, IInspectable
{
    private Vector3 targetPosition;
    private Tweener moveTweener;
    public Transform GetTransform() => stateMachine.transform;
    public ArtefactPieceMoveState(ArtefactPieceStateMachine stateMachine, Vector3 targetPos) : base(stateMachine)
    {
        targetPosition = targetPos;
    }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Move;
        moveTweener = stateMachine.transform.DOMove(targetPosition, 0.6f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
            });
    }

    public override void Tick(float deltaTime) { }
    public override void Exit() { moveTweener?.Kill(); }
    public void EnterInspect(Vector3 targetPosition) { }
    public void ExitInspect()
    {
        moveTweener?.Kill();
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
}