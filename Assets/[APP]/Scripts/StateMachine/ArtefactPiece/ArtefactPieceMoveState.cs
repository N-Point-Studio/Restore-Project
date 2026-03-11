using UnityEngine;
using DG.Tweening;

public class ArtefactPieceMoveState : ArtefactPieceBaseState, IInspectable
{
    private readonly Transform targetTransform;
    private Sequence moveSequence;
    private ArtefactPieceState temporaryState;

    // public bool IsInspected => stateMachine.isInspected;
    public Transform GetTransform() => stateMachine.transform;
    public ArtefactPieceMoveState(ArtefactPieceStateMachine stateMachine, Transform targetPos, ArtefactPieceState state) : base(stateMachine)
    {
        targetTransform = targetPos;
        temporaryState = state;
    }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Move;
        moveSequence = DOTween.Sequence();

        Debug.Log($"Enter move from {stateMachine.transform.position} to {targetTransform.position}");


        moveSequence.Join(
            stateMachine.transform.DOMove(targetTransform.position, 0.5f).SetEase(Ease.OutBack)
        );

        moveSequence.Join(
            stateMachine.transform.DORotateQuaternion(targetTransform.rotation, 0.5f).SetEase(Ease.OutBack)
        );

        moveSequence.OnComplete(() =>
        {
            switch (temporaryState)
            {
                case ArtefactPieceState.Inspect:
                    stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
                    break;
                case ArtefactPieceState.Assembled:
                    stateMachine.SwitchState(new ArtefactPieceAssembledState(stateMachine));
                    break;
            }
        });
    }

    public override void Tick(float deltaTime) { }
    public override void Exit() { moveSequence?.Kill(); }
    public void EnterInspect(Transform targetPosition) { }
    public void ExitInspect()
    {
        // stateMachine.isInspected = false;
        moveSequence?.Kill();
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
}