using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceStateMachine : StateMachine, IInteractObject, IClick, IDragObject, IHold, IAssembled, IInspectable
{
    public ArtefactPieceState state = ArtefactPieceState.None;
    public string pieceId;
    public string PieceId => pieceId;
    public List<ConnectionSocket> sockets;
    public List<ConnectionSocket> GetSockets() => sockets;
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }

    public bool isInspected;
    public bool IsInspected => this.isInspected;

    public static event Action<ArtefactPieceStateMachine> OnCreated;
    public IAssembled parent;

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

    public ArtefactPieceState GetCurrentStateEnum() { return state; }

    public void ResetTransform()
    {
        transform.SetPositionAndRotation(InitialPosition, InitialRotation);
    }

    public ConnectionSocket GetAvailableSocketFor(string otherId)
    {
        return sockets.Find(s => s.targetPieceId == otherId && !s.isOccupied);
    }

    public void OnInteractStart() => (currentState as IInteract)?.OnInteractStart();
    public void OnInteractEnd() => (currentState as IInteract)?.OnInteractEnd();
    public void OnClick() => (currentState as IClick)?.OnClick();
    // public void OnDragPerformed(Vector3 worldPos) => (currentState as IDrag)?.OnDragPerformed(worldPos);
    public void OnHoldPerformed() => (currentState as IHold)?.OnHoldPerformed();
    public void OnAssembled(IAssembled parent, Transform transform) => (currentState as IAssembled)?.OnAssembled(parent, transform);
    public void OnDetached() => (currentState as IAssembled)?.OnDetached();

    public void EnterInspect(Transform targetPosition) => (currentState as IInspectable)?.EnterInspect(targetPosition);
    public void ExitInspect() => (currentState as IInspectable)?.ExitInspect();

    public Transform GetTransform() => transform;
    public IAssembled GetAssembleParrent() => parent;

    public void ReleaseSocketWith(string otherId)
    {
        var socket = sockets.Find(s => s.targetPieceId == otherId && s.isOccupied);
        if (socket != null)
        {
            socket.isOccupied = false;
        }
    }

    public void OnInteractDetected()
    {
        // Debug.Log($"{name} interact detected");
    }

    public void OnInteractEnded()
    {
        // Debug.Log($"{name} interact notdetected");
    }


    //==new

    public void OnDragStarted(Vector3 worldPos) => (currentState as IDragObject)?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDragObject)?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => (currentState as IDragObject)?.OnDragEnded(worldPos);
}