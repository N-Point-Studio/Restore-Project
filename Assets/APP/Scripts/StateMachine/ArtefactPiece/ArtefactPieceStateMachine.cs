using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ArtefactPieceStateMachine : PartStateMachine, IInteractObject, IDragObject, IClean, IArtefactPart
{
    private Collider col;
    public ArtefactPieceState state = ArtefactPieceState.None;
    public static event Action<ArtefactPieceStateMachine> OnCreated;

    [Header("Animation Timings")]
    public float moveDuration = 0.5f;
    public float returnDuration = 0.5f;
    public float punchDuration = 0.4f;
    public Vector3 punchRotation = new Vector3(5, 5, 0);

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player artefactFeedback;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }
    private void Start()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        SwitchState(new ArtefactPieceIdleState(this));
        OnCreated?.Invoke(this);
    }

    // === Assemble ===
    public string pieceId;
    public List<ConnectionSocket> sockets;

    //=== IInteractObject, IDragObject ===
    public void OnInteractDetected() => (currentState as IInteractObject)?.OnInteractDetected();
    public void OnInteractEnded() => (currentState as IInteractObject)?.OnInteractEnded();
    public void SetColliderEnable(bool isActive) => col.enabled = isActive;
    public void OnDragStarted(Vector3 worldPos) => (currentState as IDragObject)?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDragObject)?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => (currentState as IDragObject)?.OnDragEnded(worldPos);

    //=== IAssemble ===
    public Transform GetTransform() => transform;
    public string PieceId => pieceId;
    public ArtefactPieceState CurrentState => state;
    public ConnectionSocket GetAvailableSocketFor(string id) => sockets.Find(s => s.targetPieceId == id && !s.isOccupied);
    public void OnAssembled(Transform targetTransform) => (currentState as IAssemble)?.OnAssembled(targetTransform);
    public void OnDetached() => (currentState as IAssemble)?.OnDetached();
    public void ReleaseSocketWith(string otherId)
    {
        var socket = sockets.Find(s => s.targetPieceId == otherId && s.isOccupied);
        if (socket != null) socket.isOccupied = false;
    }

    //=== IArtefactPart ===
    public List<ConnectionSocket> GetSockets() => sockets;
    public bool IsSlotEmpty()
    {
        foreach (var socket in sockets)
        {
            if (socket.isOccupied) return false;
        }
        return true;
    }
    public void CorrectRotation(Quaternion rotation) => transform.DORotateQuaternion(rotation, moveDuration).SetEase(Ease.OutCubic);

    //=== IClean ===
    public bool IsCleanable() => state == ArtefactPieceState.Assembled;
    public void ForceClean() { }
}