using System;
using UnityEngine;

public class CleaningChunk : MonoBehaviour, IInteractObject, ICleanChunk
{
    [SerializeField] private int maxHit = 3;
    private int currentHit;
    public static event Action<CleaningChunk> OnCreated;
    public static event Action<CleaningChunk> OnDestroyed;

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
}
