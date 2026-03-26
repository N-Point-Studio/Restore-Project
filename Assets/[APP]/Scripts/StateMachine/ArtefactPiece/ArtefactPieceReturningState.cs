using UnityEngine;
using DG.Tweening;

public class ArtefactPieceReturningState : ArtefactPieceBaseState
{
    private Sequence returnSequence;

    public ArtefactPieceReturningState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Returning;
        returnSequence = DOTween.Sequence();

        returnSequence.Join(
            stateMachine.transform.DOMove(stateMachine.InitialPosition, 0.5f)
                .SetEase(Ease.OutBack)
        );

        returnSequence.Join(
            stateMachine.transform.DORotateQuaternion(stateMachine.InitialRotation, 0.5f)
                .SetEase(Ease.OutBack)
        );

        returnSequence.OnComplete(() =>
        {
            stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
            AssembleEvents.OnAssembleFinished?.Invoke();
            AudioEvents.TriggerPlayAssembleSFX();
        });
    }

    public override void Tick(float deltaTime) { }

    public override void Exit()
    {
        returnSequence?.Kill();
    }
}