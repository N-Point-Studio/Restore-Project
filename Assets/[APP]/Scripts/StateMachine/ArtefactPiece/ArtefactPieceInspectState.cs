using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IRotate, IHold, IZoom, IInspectable
{
    private float rotateSpeed = .5f;
    private float zoomSpeed = 2f;
    private float minDistance = 0.5f;
    private float maxDistance = 5f;

    private float currentDistance;
    private Camera mainCam;
    public Transform Transform => stateMachine.transform;
    public ArtefactPieceInspectState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        Debug.Log($"{stateMachine.name} Entered Inspect State");
        mainCam = Camera.main;

        currentDistance = Vector3.Distance(
            mainCam.transform.position,
            stateMachine.transform.position
        );
    }

    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnStart() { }
    public void OnHoldPerformed() { }
    public void OnRotatePerformed(Vector2 delta)
    {
        float rotateY = -delta.x * rotateSpeed;
        float rotateX = delta.y * rotateSpeed;
        stateMachine.transform.Rotate(Vector3.up, rotateY, Space.World);
        stateMachine.transform.Rotate(Vector3.right, rotateX, Space.World);
    }
    public void OnZoomPerformed(float zoomDelta)
    {
        if (mainCam == null) return;

        // drag up = zoom in
        currentDistance -= zoomDelta * zoomSpeed;

        currentDistance = Mathf.Clamp(
            currentDistance,
            minDistance,
            maxDistance
        );

        stateMachine.transform.position =
            mainCam.transform.position +
            mainCam.transform.forward * currentDistance;
    }

    public void OnEnd() { }
    public void EnterInspect() { }
    public void ExitInspect()
    {
        Debug.Log("Exit state");
        stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
    }
}

