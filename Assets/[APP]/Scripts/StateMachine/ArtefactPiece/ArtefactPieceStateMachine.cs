using UnityEngine;
using VContainer;

public class ArtefactPieceStateMachine : StateMachine, IInteractable, IInspectable
{
    public float minScale = 3;
    public float maxScale = 10;
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }
    public Transform Transform => transform;

    private void Start()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        SwitchState(new ArtefactPieceIdleState(this));
    }

    public void OnStart(Vector3 worldPos, Vector2 screenPos) => (currentState as IInteractable)?.OnStart(worldPos, screenPos);
    public void OnMove(Vector3 worldPos, Vector2 screenPos) => (currentState as IInteractable)?.OnMove(worldPos, screenPos);
    public void OnEnd(Vector3 worldPos, Vector2 screenPos) => (currentState as IInteractable)?.OnEnd(worldPos, screenPos);
    public void OnHold() => (currentState as IInteractable)?.OnHold();
    public void OnZoom(float delta) => (currentState as IInteractable)?.OnZoom(delta);
    public void EnterInspect() => (currentState as IInspectable)?.EnterInspect();
    public void ExitInspect() => (currentState as IInspectable)?.ExitInspect();
    public void ResetTransform()
    {
        transform.position = InitialPosition;
        transform.rotation = InitialRotation;
    }
}