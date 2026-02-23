using UnityEngine;

public interface IDrag : IInteract
{
    void OnDragPerformed(Vector3 worldPos);
}