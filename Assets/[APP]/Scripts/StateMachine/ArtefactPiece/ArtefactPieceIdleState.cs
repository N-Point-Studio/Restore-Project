using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IClick, IDrag, IInspectable, IAssembled
{
    public Transform Transform => stateMachine.transform;

    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        stateMachine.ResetTransform();
        Debug.Log($"{stateMachine.name} Entered Idle State");
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnClick() { }
    public void OnStart()
    {
        stateMachine.ResetTransform();
    }
    public void OnDragPerformed(Vector3 worldPos)
    {
        stateMachine.transform.position = worldPos;
    }
    public void OnEnd()
    {
        stateMachine.ResetTransform();
    }
    public void EnterInspect() { stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine)); }
    public void ExitInspect() { }

    public void OnAssembled()
    {
        stateMachine.SwitchState(new ArtefactPieceAssembledState(stateMachine));
    }

    public void OnDetached()
    {
    }
}