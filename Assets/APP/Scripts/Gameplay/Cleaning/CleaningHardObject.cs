using System;
using UnityEngine;

public class CleaningHardObject : MonoBehaviour, IInteractObject, ICleanHardObject, IDragObject
{
    [SerializeField] private int maxHitTimes = 3;
    [SerializeField] private Texture[] damageTextures;
    private Renderer targetRenderer;

    private int currentHit;
    private IDragObject parentDragObject; // Reference to parent

    public static event Action<CleaningHardObject> OnCreated;
    public static event Action<CleaningHardObject> OnDestroy;

    public int CurrentHit => currentHit;
    public int MaxHit => maxHitTimes;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        
        if (transform.parent != null)
        {
            parentDragObject = transform.parent.GetComponentInParent<IDragObject>();
        }
    }

    private void Start()
    {
        OnCreated?.Invoke(this);
    }

    public void Hit()
    {
        currentHit++;

        UpdateTexture();

        if (currentHit >= maxHitTimes)
        {
            DestroyObject();
        }
    }

    private void UpdateTexture()
    {
        if (damageTextures == null || damageTextures.Length == 0) return;

        int index = Mathf.Clamp(currentHit - 1, 0, damageTextures.Length - 1);

        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = damageTextures[index];
        }
    }

    private void DestroyObject()
    {
        OnDestroy?.Invoke(this);
        Destroy(gameObject);
    }

    public void ForceClean()
    {
        currentHit = maxHitTimes;
        DestroyObject();
    }

    public void OnInteractDetected() { }
    public void OnInteractEnded() { }
    public void SetColliderEnable(bool isActive) { }

    public bool IsCleanable()
    {
        var parentPart = GetComponentInParent<ICleanSurfaceObject>();
        if (parentPart != null)
        {
            return parentPart.IsCleanable();
        }
        return false;
    }

    public void OnDragStarted(Vector3 worldPos) => parentDragObject?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => parentDragObject?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => parentDragObject?.OnDragEnded(worldPos);
}