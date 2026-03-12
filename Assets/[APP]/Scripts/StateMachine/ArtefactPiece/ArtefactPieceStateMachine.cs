using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtefactPieceStateMachine : PartStateMachine, IInteractObject, IDragObject, IInspectable, ICleanObject
{
    public ArtefactPieceState state = ArtefactPieceState.None;
    public string pieceId;
    public List<ConnectionSocket> sockets;
    public static event Action<ArtefactPieceStateMachine> OnCreated;

    // === CleaningObject ===
    [SerializeField] CleaningObject cleaning;

    private void Awake() { OnCreated?.Invoke(this); }
    private void Start()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        SwitchState(new ArtefactPieceIdleState(this));
    }

    //=== IInspectable ===
    public Transform GetTransform() => transform;
    public void EnterInspect(Transform targetPosition) => (currentState as IInspectable)?.EnterInspect(targetPosition);
    public void ExitInspect() => (currentState as IInspectable)?.ExitInspect();

    //=== IInteractObject, IDragObject ===
    public void OnInteractDetected() => (currentState as IInteractObject)?.OnInteractDetected();
    public void OnInteractEnded() => (currentState as IInteractObject)?.OnInteractEnded();
    public void OnDragStarted(Vector3 worldPos) => (currentState as IDragObject)?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => (currentState as IDragObject)?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => (currentState as IDragObject)?.OnDragEnded(worldPos);

    //=== ICleanObject ===
    public void TryClean(Vector2 uv, Texture2D brush) => cleaning.TryClean(uv, brush);
}