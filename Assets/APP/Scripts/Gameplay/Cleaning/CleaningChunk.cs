using System;
using DG.Tweening;
using UnityEngine;

public class CleaningChunk : MonoBehaviour, IInteractObject, ICleanChunk, IDragObject
{
    [SerializeField] private int maxHit = 3;

    [Header("Chunk VFX")]
    [SerializeField] private ParticleSystem debuVFX;
    [SerializeField] private ParticleSystem chunkVFX;

    private int currentHit;
    private bool isDestroying = false;

    public static event Action<CleaningChunk> OnCreated;
    public static event Action<CleaningChunk> OnDestroyed;

    private Renderer rend;
    private Collider col;
    private Material myMaterial;

    private int crackThresholdID = Shader.PropertyToID("_Treshold");

    private IDragObject parentDragObject;

    private void Awake()
    {
        if (transform.parent != null)
        {
            parentDragObject = transform.parent.GetComponentInParent<IDragObject>();
        }

        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        if (rend != null)
        {
            myMaterial = rend.material;
            myMaterial.SetFloat(crackThresholdID, 1f);
        }
    }

    private void Start()
    {
        OnCreated?.Invoke(this);

        if (debuVFX != null) debuVFX.Stop();
        if (chunkVFX != null) chunkVFX.Stop();
    }

    public void Hit()
    {
        if (isDestroying) return;

        currentHit++;


        if (debuVFX != null)
        {
            debuVFX.Stop();
            debuVFX.Play();
        }

        if (chunkVFX != null)
        {
            chunkVFX.Stop();
            chunkVFX.Play();
        }

        if (currentHit >= maxHit)
        {
            DestroyChunk();
            return;
        }

        if (myMaterial != null)
        {
            float crackValue = 1f;
            if (maxHit == 2)
            {
                crackValue = 0f;
            }
            else
            {
                if (currentHit == 1)
                {
                    crackValue = 0.3f;
                }
                else
                {
                    float t = (float)(currentHit - 1) / (maxHit - 2);
                    crackValue = Mathf.Lerp(0.3f, 0f, t);
                }
            }

            myMaterial.SetFloat(crackThresholdID, crackValue);
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
        if (isDestroying) return;
        isDestroying = true;

        OnDestroyed?.Invoke(this);

        if (rend != null) rend.enabled = false;
        if (col != null) col.enabled = false;

        float destroyDelay = 2f;

        if (debuVFX != null)
        {
            debuVFX.Play();
            destroyDelay = Mathf.Max(destroyDelay, debuVFX.main.duration + debuVFX.main.startLifetime.constantMax);
        }

        if (chunkVFX != null)
        {
            chunkVFX.Play();
            destroyDelay = Mathf.Max(destroyDelay, chunkVFX.main.duration + chunkVFX.main.startLifetime.constantMax);
        }

        Destroy(gameObject, destroyDelay);
    }

    private void OnDestroy()
    {
        if (myMaterial != null)
        {
            Destroy(myMaterial);
        }
    }

    public void OnInteractDetected() { }
    public void OnInteractEnded() { }

    public void SetColliderEnable(bool isActive)
    {
        if (col != null && !isDestroying)
        {
            col.enabled = isActive;
        }
    }

    public void ForceClean() { DestroyChunk(); }

    public void OnDragStarted(Vector3 worldPos) => parentDragObject?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => parentDragObject?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => parentDragObject?.OnDragEnded(worldPos);
}