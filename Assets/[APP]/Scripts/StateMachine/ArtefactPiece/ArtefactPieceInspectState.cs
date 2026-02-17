using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IRotate, IHold, IZoom, IInspectable
{
    private float rotateSpeed = .5f;
    public Transform Transform => stateMachine.transform;
    public ArtefactPieceInspectState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter() { Debug.Log($"{stateMachine.name} Entered Inspect State"); }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnStart() { }
    public void OnHoldPerformed()
    {
        stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
        // Debug.Log($"{stateMachine.name} Hold");
    }

    public void OnRotatePerformed(Vector2 delta)
    {
        // Debug.Log("Rotate from inspect");
        float rotateY = -delta.x * rotateSpeed;
        float rotateX = delta.y * rotateSpeed;
        stateMachine.transform.Rotate(Vector3.up, rotateY, Space.World);
        stateMachine.transform.Rotate(Vector3.right, rotateX, Space.World);
    }

    public void OnZoomPerformed()
    {
        // Debug.Log($"{stateMachine.name} Zoom");
    }
    public void OnEnd() { }

    public void EnterInspect() { }
    public void ExitInspect() { stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine)); }
}

