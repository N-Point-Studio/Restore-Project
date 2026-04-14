using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArtefactPieceStateMachine : PartStateMachine, IInteractObject, IDragObject, ICleanSurfaceObject, IArtefactPart
{
    private Collider col;
    private Renderer rd;
    public ArtefactPieceState state = ArtefactPieceState.None;
    public static event Action<ArtefactPieceStateMachine> OnCreated;

    [Header("Animation Timings")]
    public float moveDuration = 0.5f;
    public float returnDuration = 0.5f;
    public float punchDuration = 0.4f;
    public Vector3 punchRotation = new Vector3(5, 5, 0);

    private void Awake()
    {
        col = GetComponent<Collider>();
        rd = GetComponent<Renderer>();
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

    // === CleaningObject ===
    [SerializeField] CleaningSurfaceObject cleaning;


    //=== IInteractObject, IDragObject ===
    public void OnInteractDetected() => (currentState as IInteractObject)?.OnInteractDetected();
    public void OnInteractEnded() => (currentState as IInteractObject)?.OnInteractEnded();
    public void OnDragStarted(Vector3 worldPos) => (currentState as IDragObject)?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDragObject)?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => (currentState as IDragObject)?.OnDragEnded(worldPos);

    //=== ICleanObject ===
    public void TryClean(Vector2 uv, Texture2D brush)
    {
        if (state != ArtefactPieceState.Idle) cleaning.TryClean(uv, brush);
    }

    //=== IAssemble ===
    public Transform GetTransform() => transform;
    public string PieceId => pieceId;
    public ArtefactPieceState CurrentState => state;
    public ConnectionSocket GetAvailableSocketFor(string id)
    {
        return sockets.Find(s => s.targetPieceId == id && !s.isOccupied);
    }
    public void OnAssembled(Transform targetTransform) => (currentState as IAssemble)?.OnAssembled(targetTransform);
    public void OnDetached() => (currentState as IAssemble)?.OnDetached();
    public void ReleaseSocketWith(string otherId)
    {
        var socket = sockets.Find(s => s.targetPieceId == otherId && s.isOccupied);
        if (socket != null) socket.isOccupied = false;
    }

    public void SetColliderEnable(bool isActive)
    {
        col.enabled = isActive;
    }

    public List<ConnectionSocket> GetSockets() => sockets;

    public void CorrectRotation(Quaternion rotation)
    {
        transform.DORotateQuaternion(rotation, moveDuration).SetEase(Ease.OutCubic);
    }

    public bool IsCleanable()
    {
        return state == ArtefactPieceState.Assembled;
    }
}