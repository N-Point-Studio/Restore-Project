using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceStateMachine : StateMachine, IInteractObject, IDragObject, IInspectable, ICleanObject, IArtefactPart
{
    public ArtefactPieceState state = ArtefactPieceState.None;
    public string pieceId;
    public string PieceId => pieceId;
    public List<ConnectionSocket> sockets;
    // public List<ConnectionSocket> GetSockets() => sockets;
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }

    [SerializeField] CleaningObject cleaning;

    public static event Action<ArtefactPieceStateMachine> OnCreated;
    public IAssembled parent;
    private IAssemble assembleParent;

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
        transform.SetPositionAndRotation(InitialPosition, InitialRotation);
    }

    // public ConnectionSocket GetAvailableSocketFor(string otherId)
    // {
    //     return sockets.Find(s => s.targetPieceId == otherId && !s.isOccupied);
    // }

    // public void OnAssembled(IAssembled parent, Transform transform) => (currentState as IAssembled)?.OnAssembled(parent, transform);
    // public void OnDetached() => (currentState as IAssembled)?.OnDetached();

    public void EnterInspect(Transform targetPosition) => (currentState as IInspectable)?.EnterInspect(targetPosition);
    public void ExitInspect() => (currentState as IInspectable)?.ExitInspect();

    public Transform GetTransform() => transform;
    // public IAssembled GetAssembleParrent() => parent;

    // public void ReleaseSocketWith(string otherId)
    // {
    //     var socket = sockets.Find(s => s.targetPieceId == otherId && s.isOccupied);
    //     if (socket != null)
    //     {
    //         socket.isOccupied = false;
    //     }
    // }

    //==new

    public void OnInteractDetected() { }
    public void OnInteractEnded() { }
    public void OnDragStarted(Vector3 worldPos) => (currentState as IDragObject)?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDragObject)?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => (currentState as IDragObject)?.OnDragEnded(worldPos);

    public void TryClean(Vector2 uv, Texture2D brush)
    {
        cleaning.TryClean(uv, brush);
    }

    //assemble

    public List<ConnectionSocket> GetSockets() => sockets;

    public ConnectionSocket GetAvailableSocketFor(string otherId)
    {
        return sockets.Find(s => s.targetPieceId == otherId && !s.isOccupied);
    }

    public void ReleaseSocketWith(string otherId)
    {
        var socket = sockets.Find(s => s.targetPieceId == otherId && s.isOccupied);
        if (socket != null)
        {
            socket.isOccupied = false;
        }
    }

    public IAssemble GetAssembleParent() => assembleParent;

    public void OnAssembled(IAssemble parent, Transform targetTransform) => (currentState as IAssemble)?.OnAssembled(parent, transform);
    //this.parent = newParent;
    public void OnDetached() => (currentState as IAssemble)?.OnDetached();
    //this.parent = null;
    public IAssemble GetRoot()
    {
        return parent == null ? this : assembleParent.GetRoot();
    }
}