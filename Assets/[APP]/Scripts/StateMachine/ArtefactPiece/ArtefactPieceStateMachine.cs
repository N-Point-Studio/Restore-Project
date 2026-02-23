using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ArtefactPieceState
{
    None,
    Idle,
    Inspect,
    Assembled,
    Returning
}

public class ArtefactPieceStateMachine : StateMachine, IClick, IDrag, IHold, IAssembled, IInspectable
{
    public ArtefactPieceState state = ArtefactPieceState.None;
    public string pieceId;
    public List<ConnectionSocket> sockets;
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }
    public static event Action<ArtefactPieceStateMachine> OnCreated;
    public ArtefactPieceStateMachine parent;
    public float returningSpeed = 5f;

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

    public State GetCurrentState() { return currentState; }
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
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDrag)?.OnDragPerformed(worldPos);
    public void OnHoldPerformed() => (currentState as IHold)?.OnHoldPerformed();

    public bool IsInspectable => currentState is IInspectable;
    public Transform Transform => transform;
    public IInspectable GetInspectable() => currentState as IInspectable;

    public void OnAssembled(ArtefactPieceStateMachine parent) => (currentState as IAssembled)?.OnAssembled(parent);
    public void OnDetached() => (currentState as IAssembled)?.OnDetached();

    public void EnterInspect() => (currentState as IInspectable)?.EnterInspect();
    public void ExitInspect() => (currentState as IInspectable)?.ExitInspect();
}