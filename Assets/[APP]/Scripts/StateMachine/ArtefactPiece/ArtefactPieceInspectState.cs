using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IRotate, IHold, IZoom, IInspectable, IAssembled
{
    private float rotateSpeed = .5f;
    public Transform Transform => stateMachine.transform;

    public ArtefactPieceInspectState(ArtefactPieceStateMachine stateMachine)
        : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Inspect;

        Debug.Log($"{stateMachine.name} Entered Inspect State");
    }

    public override void Tick(float deltaTime)
    {
    }

    public override void Exit() { }

    public void OnInteractStart() { }
    public void OnHoldPerformed() { }

    public bool OnRotatePerformed(Vector2 delta)
    {
        float rotateY = -delta.x * rotateSpeed;
        float rotateX = delta.y * rotateSpeed;

        stateMachine.transform.Rotate(Vector3.up, rotateY, Space.World);
        stateMachine.transform.Rotate(Vector3.right, rotateX, Space.World);

        return true;
    }

    public void OnZoomPerformed(float zoomDelta)
    {
    }

    public void OnInteractEnd() { }

    public void EnterInspect()
    {
        Debug.Log("Enter Inspect (from inspect): " + stateMachine.name);
    }

    public void ExitInspect()
    {
        Debug.Log("Exit state");
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }

    public void OnAssembled(ArtefactPieceStateMachine parent)
    {
        Debug.Log($"{stateMachine.name} Masuk assembled state");
    }

    public void OnDetached()
    {
        Debug.Log("Detached");
        stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
    }
}