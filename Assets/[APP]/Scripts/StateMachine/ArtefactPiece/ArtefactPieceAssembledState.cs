using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IHold, IAssembled
{
    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Assembled;
        Debug.Log($"{stateMachine.name} Entered Assembled State");
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void OnInteractStart()
    {
    }

    public void OnInteractEnd()
    {
    }

    public void OnHoldPerformed()
    {
    }

    public void OnAssembled(ArtefactPieceStateMachine parent)
    {
        stateMachine.parent = parent;
    }

    public void OnDetached()
    {
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }
}
