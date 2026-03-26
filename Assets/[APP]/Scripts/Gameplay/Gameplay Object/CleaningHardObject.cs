using System;
using UnityEngine;

public class CleaningHardObject : MonoBehaviour, IInteractObject, ICleanHardObject
{
    [SerializeField] private int maxHitTimes = 3;
    [SerializeField] private Texture[] damageTextures;
    private Renderer targetRenderer;

    private int currentHit;

    public static event Action<CleaningHardObject> OnCreated;
    public static event Action<CleaningHardObject> OnDestroy;

    public int CurrentHit => currentHit;
    public int MaxHit => maxHitTimes;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        OnCreated?.Invoke(this);
    }

    public void Hit()
    {
        Debug.Log($"Hit {name} loh ya!");
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

    public void OnInteractDetected()
    {
        // Debug.Log($"Hard object {name} detected");
    }

    public void OnInteractEnded()
    {
    }

    public void SetColliderEnable(bool isActive)
    {
    }

    public bool IsCleanable()
    {
        var parentPart = GetComponentInParent<ICleanSurfaceObject>();
        if (parentPart != null)
        {
            return parentPart.IsCleanable();
        }
        return false;
    }
}
