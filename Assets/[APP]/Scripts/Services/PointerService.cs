using UnityEngine;

public class PointerService
{
    private readonly Camera cam;

    public PointerService(Camera cam)
    {
        this.cam = cam;
    }

    public bool TryRaycast(Vector2 screenPos, out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit);
    }

    public Vector3 ScreenToWorld(Vector2 screenPos, float depth)
    {
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
    }

    public float GetDepth(Vector3 worldPos)
    {
        return cam.WorldToScreenPoint(worldPos).z;
    }
}