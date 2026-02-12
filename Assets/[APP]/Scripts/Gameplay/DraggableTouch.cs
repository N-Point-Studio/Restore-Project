using UnityEngine;

public class DraggableTouch : MonoBehaviour, IDraggable
{
    public void OnDragStart(Vector3 worldPosition)
    {
        Debug.Log($"{name} Start Drag");
    }

    public void OnDragPerformed(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public void OnDragEnd()
    {
        Debug.Log($"{name} End Drag");
    }
}
