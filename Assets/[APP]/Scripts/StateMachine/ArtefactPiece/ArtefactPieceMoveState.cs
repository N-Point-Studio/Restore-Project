using UnityEngine;
using DG.Tweening;

public class ArtefactPieceMoveState : ArtefactPieceBaseState
{
    private readonly Transform targetTransform;
    private Sequence moveSequence;

    public ArtefactPieceMoveState(ArtefactPieceStateMachine stateMachine, Transform targetPos) : base(stateMachine)
    {
        targetTransform = targetPos;
    }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Move;
        moveSequence = DOTween.Sequence();

        moveSequence.Join(
            stateMachine.transform.DOMove(targetTransform.position, 0.5f).SetEase(Ease.OutBack)
        );

        moveSequence.Join(
            stateMachine.transform.DORotateQuaternion(targetTransform.rotation, 0.5f).SetEase(Ease.OutBack)
        );

        moveSequence.OnComplete(() =>
        {
            stateMachine.SwitchState(new ArtefactPieceAssembledState(stateMachine));
        });
    }

    public override void Tick(float deltaTime) { }
    public override void Exit() { moveSequence?.Kill(); }
}