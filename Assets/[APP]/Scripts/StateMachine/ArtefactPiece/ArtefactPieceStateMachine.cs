using UnityEngine;
using VContainer;

public class ArtefactPieceStateMachine : StateMachine, IClick, IDrag, IRotate, IHold, IZoom
{
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }

    private void Start()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        SwitchState(new ArtefactPieceIdleState(this));
    }

    public void ResetTransform()
    {
        transform.position = InitialPosition;
        transform.rotation = InitialRotation;
    }

    public bool IsInspectable => currentState is IInspectable;
    public IInspectable GetInspectable() => currentState as IInspectable;

    public void OnClick() => (currentState as IClick)?.OnClick();
    public void OnStart() => (currentState as IInteract)?.OnStart();
    public void OnEnd() => (currentState as IInteract)?.OnEnd();
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDrag)?.OnDragPerformed(worldPos);
    public void OnRotatePerformed(Vector2 delta) => (currentState as IRotate)?.OnRotatePerformed(delta);
    public void OnHoldPerformed() => (currentState as IHold)?.OnHoldPerformed();
    public void OnZoomPerformed() => (currentState as IZoom)?.OnZoomPerformed();
}