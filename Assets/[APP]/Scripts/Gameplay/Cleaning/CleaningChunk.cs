using System;
using UnityEngine;

public class CleaningChunk : MonoBehaviour, IInteractObject, ICleanChunk, IDragObject
{
    [SerializeField] private int maxHit = 3;
    private int currentHit;
    public static event Action<CleaningChunk> OnCreated;
    public static event Action<CleaningChunk> OnDestroyed;

    // ADDED: Parent reference for dragging
    private IDragObject parentDragObject; 

    private void Awake()
    {
        // ADDED: Find the parent Artefact safely
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
        if (currentHit >= maxHit)
        {
            DestroyChunk();
        }
    }

    public bool IsCleanable()
    {
        var parent = GetComponentInParent<ICleanSurface>();
        if (parent != null) return parent.IsCleanable();
        return false;
    }

    private void DestroyChunk()
    {
        OnDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void OnInteractDetected() { }
    public void OnInteractEnded() { }
    public void SetColliderEnable(bool isActive) { }
    public void ForceClean() { DestroyChunk(); }

    // --- ADDED: Forward IDragObject commands to the Artefact ---
    public void OnDragStarted(Vector3 worldPos) => parentDragObject?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => parentDragObject?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => parentDragObject?.OnDragEnded(worldPos);
}