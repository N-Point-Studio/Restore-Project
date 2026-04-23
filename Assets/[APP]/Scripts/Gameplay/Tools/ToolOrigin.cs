using UnityEngine;

public class ToolOrigin : MonoBehaviour, IInteractObject
{
    private Collider col;
    private Renderer rend;
    void Start()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        col.enabled = false;
        rend.enabled = false;
    }

    public void SetCollider(bool isActive)
    {
        col.enabled = isActive;
        rend.enabled = isActive;
    }

    public void OnInteractDetected()
    {
    }

    public void OnInteractEnded()
    {
    }

    public void SetColliderEnable(bool isActive)
    {
    }
}
