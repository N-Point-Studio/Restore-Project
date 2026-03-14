using UnityEngine;

public class CleaningHardObject : MonoBehaviour, IInteractObject, ICleanHardObject
{
    [SerializeField] private int maxHitTimes = 3;
    [SerializeField] private Texture[] damageTextures;
    private Renderer targetRenderer;

    private int currentHit;

    public int CurrentHit => currentHit;
    public int MaxHit => maxHitTimes;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
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
}
