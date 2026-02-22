using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IClick, IDrag, IInspectable, IAssembled
{
    public Transform Transform => stateMachine.transform;

    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.ResetTransform();
        stateMachine.state = ArtefactPieceState.Idle;
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public void OnInteractStart() { stateMachine.ResetTransform(); }
    public void OnInteractEnd() { stateMachine.ResetTransform(); }

    public void OnClick() { }

    public bool OnDragPerformed(Vector3 worldPos)
    {
        stateMachine.transform.position = worldPos;
        return true;
    }

    public void EnterInspect() { stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine)); }
    public void ExitInspect() { }

    public void OnAssembled(ArtefactPieceStateMachine parent)
    {
        Debug.Log($"di idle masuk ke {parent.name}");
        stateMachine.parent = parent;
        stateMachine.SwitchState(new ArtefactPieceAssembledState(stateMachine));
    }

    public void OnDetached() { }
}