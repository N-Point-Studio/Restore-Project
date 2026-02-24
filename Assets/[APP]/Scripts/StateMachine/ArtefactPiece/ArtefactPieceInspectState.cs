using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IHold, IInspectable, IAssembled
{
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
    public void EnterInspect() { }

    public void ExitInspect()
    {
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
    public void OnAssembled(ArtefactPieceStateMachine parent) { }

    public void OnDetached()
    {
        stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
    }
}