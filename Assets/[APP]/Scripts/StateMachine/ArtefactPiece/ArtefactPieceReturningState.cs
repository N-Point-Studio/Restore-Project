using UnityEngine;

public class ArtefactPieceReturningState : ArtefactPieceBaseState
{
    public ArtefactPieceReturningState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Returning;
    }

    public override void Tick(float deltaTime)
    {
        stateMachine.transform.position = Vector3.Lerp(
            stateMachine.transform.position,
            stateMachine.InitialPosition,
            deltaTime * stateMachine.returningSpeed
        );

        stateMachine.transform.rotation = Quaternion.Lerp(
            stateMachine.transform.rotation,
            stateMachine.InitialRotation,
            deltaTime * stateMachine.returningSpeed
        );

        if (Vector3.Distance(stateMachine.transform.position, stateMachine.InitialPosition) < 0.01f)
        {
            stateMachine.transform.position = stateMachine.InitialPosition;
            stateMachine.transform.rotation = stateMachine.InitialRotation;
            stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
        }
    }

    public override void Exit() { }
}
