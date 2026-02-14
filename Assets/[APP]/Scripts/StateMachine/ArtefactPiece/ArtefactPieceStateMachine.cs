using UnityEngine;
using VContainer;

public class ArtefactPieceStateMachine : StateMachine, IDraggable, IInspectable
{
    public ArtefactCluster Cluster { get; private set; }
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }

    private void Start()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        SwitchState(new ArtefactPieceIdleState(this));
    }

    public Transform Transform => transform;
    public void OnAssembled() => (currentState as IAssemblable)?.OnAssembled();
    public void OnDragStart(Vector3 worldPosition) => (currentState as IDraggable)?.OnDragStart(worldPosition);
    public void OnDisassembled() => (currentState as IAssemblable)?.OnDisassembled();
    public void OnDragPerformed(Vector3 worldPosition) => (currentState as IDraggable)?.OnDragPerformed(worldPosition);
    public void OnDragEnd(Vector3 worldPosition) => (currentState as IDraggable)?.OnDragEnd(worldPosition);
    public void EnterInspect() => (currentState as IInspectable)?.EnterInspect();
    public void ExitInspect() => (currentState as IInspectable)?.ExitInspect();
}
