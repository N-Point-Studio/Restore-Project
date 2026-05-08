using UnityEngine;

public abstract class ArtefactPieceBaseState : State
{
    protected ArtefactPieceStateMachine stateMachine;
    public ArtefactPieceBaseState(ArtefactPieceStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
}
