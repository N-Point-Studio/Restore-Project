using UnityEngine;

public class ArtefactPieceStateMachine : StateMachine, IDraggable
{
    public ArtefactCluster Cluster { get; private set; }
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }

    public void OnDragEnd(Vector3 worldPosition) => (currentState as IDraggable)?.OnDragEnd(worldPosition);

    public void OnDragPerformed(Vector3 worldPosition) => (currentState as IDraggable)?.OnDragPerformed(worldPosition);

    public void OnDragStart(Vector3 worldPosition) => (currentState as IDraggable)?.OnDragEnd(worldPosition);

    private void Start()
    {
        SwitchState(new ArtefactPieceIdleState(this));
    }
}
