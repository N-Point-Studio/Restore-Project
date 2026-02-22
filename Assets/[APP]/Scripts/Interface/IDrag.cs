using UnityEngine;

public interface IDrag : IInteract
{
    bool OnDragPerformed(Vector3 worldPos);
}