using UnityEngine;

public class ArtefactPieceIdleState : ArtefactPieceBaseState, IInteractable, IInspectable
{
    private Vector2 startScreenPos;
    private bool hasMoved;
    private const float ClickThreshold = 10f; // Dalam pixels

    public Transform Transform => throw new System.NotImplementedException();

    public ArtefactPieceIdleState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter() { }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }
    public void OnStart(Vector3 worldPos, Vector2 screenPos)
    {
        startScreenPos = screenPos;
        hasMoved = false;
    }

    public void OnMove(Vector3 worldPos, Vector2 screenPos)
    {
        if (!hasMoved && Vector2.Distance(screenPos, startScreenPos) > ClickThreshold)
        {
            hasMoved = true;
        }
        if (hasMoved)
        {
            stateMachine.transform.position = worldPos;
        }
    }

    public void OnEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (!hasMoved)
        {
            HandleClick();
        }
        else
        {
            HandleDrop(worldPos);
        }
    }

    private void HandleClick()
    {
        Debug.Log("Object Clicked! Switching to Inspect Mode.");
        // stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
    }

    private void HandleDrop(Vector3 finalPos)
    {
        Debug.Log("Object Dropped.");
    }
    public void OnHold() { }
    public void OnZoom(float delta) { }

    public void EnterInspect()
    {
        Debug.Log("masuk inspect");
    }

    public void ExitInspect()
    {
    }
}