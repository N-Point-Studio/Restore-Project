using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IHold
{
    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log($"{stateMachine.name} Entered Assembled State");
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnStart()
    {
    }
    public void OnEnd()
    {
    }
    public void OnHoldPerformed()
    {
        stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
    }

}
