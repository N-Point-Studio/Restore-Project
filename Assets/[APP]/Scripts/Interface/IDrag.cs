using UnityEngine;

public interface IDrag : IInteract
{
    void OnDragPerformed(Vector3 worldPos);
}

public interface IDragObject
{
    void OnDragStarted(Vector3 worldPos);
    void OnDragPerformed(Vector3 worldPos);
    void OnDragEnded(Vector3 worldPos);
}