using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IHold, IAssembled, IInspectable
{
    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Assembled;
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnInteractStart() { }
    public void OnInteractEnd() { }
    public void OnHoldPerformed() { }

    public void OnAssembled(ArtefactPieceStateMachine parent)
    {
        stateMachine.parent = parent;
    }

    public void OnDetached()
    {
        stateMachine.parent = null;
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }

    public void EnterInspect()
    {
        stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
    }

    public void ExitInspect() { }
}
