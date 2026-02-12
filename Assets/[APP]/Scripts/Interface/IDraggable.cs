using UnityEngine;

public interface IDraggable
{
    void OnDragStart(Vector3 worldPosition);
    void OnDrag(Vector3 worldPosition);
    void OnDragEnd();
}