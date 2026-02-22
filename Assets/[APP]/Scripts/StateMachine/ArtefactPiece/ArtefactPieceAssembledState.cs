using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IHold, IDrag, IRotate, IAssembled
{
    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Assembled;
        Debug.Log($"{stateMachine.name} Entered Assembled State");
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnStart()
    {
    }
    public void OnEnd()
    {
        // if (stateMachine.parent.GetCurrentState() is IDrag dragable)
        // {
        //     dragable.OnEnd();
        // }
    }
    public void OnHoldPerformed()
    {
        if (CanPerformDetach())
        {
            stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
            stateMachine.parent = null;
        }
    }
    public bool CanPerformDetach()
    {
        return stateMachine.parent.GetCurrentState() is ArtefactPieceInspectState;
    }
    public bool OnDragPerformed(Vector3 worldPos)
    {
        return false;
        // var isDraggable = 
        // if (stateMachine.parent.GetCurrentState() is IDrag dragable)
        // {
        //     dragable.OnDragPerformed(worldPos);
        // }
        // Debug.Log($"[Assemble] Drag {stateMachine.GetRoot().name} is draggable {isDraggable}");
    }

    public bool OnRotatePerformed(Vector2 delta)
    {
        return false;
        // if (stateMachine.parent.GetCurrentState() is IRotate rotate)
        // {
        //     rotate.OnRotatePerformed(delta);
        // }

        // Debug.Log($"[Assemble] Rotate {stateMachine.parent.name} is rotatable {isRotatable}");
    }

    public void OnAssembled(ArtefactPieceStateMachine parent)
    {
        stateMachine.parent = parent;
    }

    public void OnDetached()
    {

    }
}
