using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ArtefactPieceState
{
    None,
    Idle,
    Inspect,
    Assembled
}

public class ArtefactPieceStateMachine : StateMachine,
IClick, IDrag, IRotate, IHold, IZoom, IAssembled
{
    public ArtefactPieceState state = ArtefactPieceState.None;
    public string pieceId;
    public List<ConnectionSocket> sockets;
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }
    public static event Action<ArtefactPieceStateMachine> OnCreated;
    public ArtefactPieceStateMachine parent;

    private void Awake()
    {
        OnCreated?.Invoke(this);
    }
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

    public ConnectionSocket GetAvailableSocketFor(string otherId)
    {
        return sockets.Find(s => s.targetPieceId == otherId && !s.isOccupied);
    }

    public bool IsInspectable => currentState is IInspectable;
    public IInspectable GetInspectable() => currentState as IInspectable;
    public void OnClick() => (currentState as IClick)?.OnClick();
    public void OnStart() => (currentState as IInteract)?.OnStart();
    public void OnEnd() => (currentState as IInteract)?.OnEnd();
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDrag)?.OnDragPerformed(worldPos);
    public void OnRotatePerformed(Vector2 delta) => (currentState as IRotate)?.OnRotatePerformed(delta);
    public void OnHoldPerformed() => (currentState as IHold)?.OnHoldPerformed();
    public void OnZoomPerformed(float zoomDelta) => (currentState as IZoom)?.OnZoomPerformed(zoomDelta);
    public void OnAssembled(ArtefactPieceStateMachine parent) => (currentState as IAssembled)?.OnAssembled(parent);
    public void OnDetached() => (currentState as IAssembled)?.OnDetached();
    public State GetCurrentState() { return currentState; }
}