using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IDraggable, IInspectable
{
    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter() { }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnDragStart(Vector3 worldPosition)
    {
        Debug.Log($"Idle clicked");
    }
    public void OnDragPerformed(Vector3 worldPosition)
    {
        Debug.Log($"Drag performed");
        stateMachine.transform.position = worldPosition;
    }
    public void OnDragEnd(Vector3 worldPosition)
    {
        Debug.Log("Drag end");
    }
    public void EnterInspect() { Debug.Log("Enter inspect"); }
    public void ExitInspect() { Debug.Log("Exit inspect"); }
}
