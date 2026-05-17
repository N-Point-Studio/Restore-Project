using UnityEngine;

public class ToolOrigin : MonoBehaviour, IInteractObject
{
    private Collider col;
    private Renderer rend;
    [SerializeField] private Tool toolObject;

    void Start()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        col.enabled = false;
        rend.enabled = false;
    }
    public void OnInteractDetected() { }
    public void OnInteractEnded() { }
    public void SetColliderEnable(bool isActive)
    {
        col.enabled = isActive;
        rend.enabled = isActive;
    }
}
