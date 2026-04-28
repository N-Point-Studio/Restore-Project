using System;
using UnityEngine;

public class CleaningChunk : MonoBehaviour, IInteractObject, ICleanChunk, IDragObject
{
    [SerializeField] private int maxHit = 3;

    [Header("Thickness Settings")]
    [SerializeField] private float minThickness = 0.05f;
    [SerializeField] private float maxThickness = 0.1f;

    [Header("Chunk VFX")]
    [SerializeField] private ParticleSystem debuVFX;
    [SerializeField] private ParticleSystem chunkVFX;

    private int currentHit;
    private bool isDestroying = false; // Penanda agar tidak di-hit double

    public static event Action<CleaningChunk> OnCreated;
    public static event Action<CleaningChunk> OnDestroyed;

    private Renderer rend;
    private Collider col; // Tambahkan referensi collider
    private Material myMaterial;
    int thicknessFalloff = Shader.PropertyToID("_ThicknessFalloff");

    // Parent reference for dragging
    private IDragObject parentDragObject;

    private void Awake()
    {
        if (transform.parent != null)
        {
            parentDragObject = transform.parent.GetComponentInParent<IDragObject>();
        }

        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>(); // Ambil komponen collider

        if (rend != null)
        {
            myMaterial = rend.material;
        }
    }

    private void Start()
    {
        OnCreated?.Invoke(this);

        // Pastikan VFX tidak menyala di awal (jaga-jaga jika Play On Awake aktif di Inspector)
        if (debuVFX != null) debuVFX.Stop();
        if (chunkVFX != null) chunkVFX.Stop();
    }

    public void Hit()
    {
        // Jika sedang dalam proses hancur, abaikan hit
        if (isDestroying) return;

        currentHit++;

        if (myMaterial != null)
        {
            float t = 1f;
            if (maxHit > 1)
            {
                t = (float)(currentHit - 1) / (maxHit - 1);
            }

            float currentFalloff = Mathf.Lerp(minThickness, maxThickness, t);
            myMaterial.SetFloat(thicknessFalloff, currentFalloff);
        }

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
        if (isDestroying) return;
        isDestroying = true; // Tandai bahwa objek sedang proses hancur

        // Beritahu sistem bahwa chunk ini sudah hancur/dibersihkan
        OnDestroyed?.Invoke(this);

        // 1. Sembunyikan Visual
        if (rend != null) rend.enabled = false;

        // 2. Matikan Collision
        if (col != null) col.enabled = false;

        // 3. Play VFX
        float destroyDelay = 2f; // Default delay jika VFX kosong

        if (debuVFX != null)
        {
            debuVFX.Play();
            // Ambil durasi terpanjang dari partikel untuk delay destroy
            destroyDelay = Mathf.Max(destroyDelay, debuVFX.main.duration + debuVFX.main.startLifetime.constantMax);
        }

        if (chunkVFX != null)
        {
            chunkVFX.Play();
            destroyDelay = Mathf.Max(destroyDelay, chunkVFX.main.duration + chunkVFX.main.startLifetime.constantMax);
        }

        // 4. Hancurkan GameObject setelah VFX selesai
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

    // Forward IDragObject commands to the Artefact
    public void OnDragStarted(Vector3 worldPos) => parentDragObject?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => parentDragObject?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => parentDragObject?.OnDragEnded(worldPos);
}