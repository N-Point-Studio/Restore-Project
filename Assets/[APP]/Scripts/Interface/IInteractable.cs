using UnityEngine;

public interface IInteractable
{
    void OnStart(Vector3 worldPos, Vector2 screenPos);
    void OnMove(Vector3 worldPos, Vector2 screenPos);
    void OnEnd(Vector3 worldPos, Vector2 screenPos);
    void OnHold();
    void OnZoom(float delta);
}
