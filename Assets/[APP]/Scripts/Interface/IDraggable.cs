using UnityEngine;

public interface IDraggable
{
    void OnDragStart(Vector3 worldPosition);
    void OnDragPerformed(Vector3 worldPosition);
    void OnDragEnd();
}