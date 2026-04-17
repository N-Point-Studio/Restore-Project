using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering; // WAJIB DITAMBAHKAN untuk CommandBuffer

public class MousePainter : MonoBehaviour, IInteractObject, ICleanSurfaceObject
{
    public Camera cam;
    public Shader paintingShader;

    [Range(0.01f, 1f)] public float brushRadius = 0.1f;
    [Range(0f, 1f)] public float brushHardness = 0.5f;

    private RenderTexture paintingMap;
    private Material paintingMaterial;
    private Material paintableMaterial;
    private RaycastHit hit;

    private Renderer rend;
    private Mesh mesh;

    // --- TAMBAHKAN INI ---
    private CommandBuffer cb;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;

        paintingMaterial = new Material(paintingShader);
        paintableMaterial = rend.material;

        // Gunakan format R8 untuk menghemat memori (hanya butuh hitam putih)
        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.R8);
        paintingMap.Create();

        // Inisialisasi CommandBuffer
        cb = new CommandBuffer();
        cb.name = "WorldSpacePainter";

        // Clear texture dengan warna hitam solid di awal menggunakan CB
        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);
        Graphics.ExecuteCommandBuffer(cb); // Eksekusi pembersihan
        cb.Clear(); // Bersihkan list perintah setelah dieksekusi

        paintableMaterial.SetTexture("_Mask", paintingMap);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject != gameObject) return;

                // Set parameter shader
                paintingMaterial.SetVector("_PaintPosition", hit.point);
                paintingMaterial.SetVector("_PaintDirection", ray.direction);
                paintingMaterial.SetFloat("_Radius", brushRadius);
                paintingMaterial.SetFloat("_Hardness", brushHardness);

                // --- INTI PERUBAHAN COMMAND BUFFER ---

                // 1. Bersihkan perintah lama yang mungkin masih nyangkut
                cb.Clear();

                // 2. Arahkan GPU untuk menggambar ke RenderTexture kita
                cb.SetRenderTarget(paintingMap);

                // 3. Masukkan perintah menggambar Mesh
                // Parameter pass = 0 (karena shader kita hanya punya 1 pass)
                cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);

                // 4. Eksekusi semua perintah di atas dengan aman
                Graphics.ExecuteCommandBuffer(cb);
            }
        }
    }

    public void ReceivePaint(Vector3 hitPoint, Vector3 hitNormal, float radius, float hardness)
    {
        paintingMaterial.SetVector("_PaintPosition", hitPoint);

        // --- TRIK PENTING ---
        // Kita jadikan 'kebalikan arah hitNormal' sebagai _PaintDirection.
        // Ini memastikan culling di shader bekerja sempurna: 
        // cat tidak akan bocor ke belakang tembok yang sedang digosok spons!
        paintingMaterial.SetVector("_PaintDirection", -hitNormal);

        paintingMaterial.SetFloat("_Radius", radius);
        paintingMaterial.SetFloat("_Hardness", hardness);

        cb.Clear();
        cb.SetRenderTarget(paintingMap);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    }

    // --- PENTING UNTUK MOBILE/iOS DEV ---
    private void OnDestroy()
    {
        // Selalu bebaskan memori GPU saat objek hancur atau pindah scene
        if (paintingMap != null)
        {
            paintingMap.Release();
            Destroy(paintingMap);
        }

        if (cb != null)
        {
            cb.Release();
        }

        if (paintingMaterial != null)
        {
            Destroy(paintingMaterial);
        }
    }

    public void OnInteractDetected()
    {
        // throw new System.NotImplementedException();
    }

    public void OnInteractEnded()
    {
        // throw new System.NotImplementedException();
    }

    public void SetColliderEnable(bool isActive)
    {
        // throw new System.NotImplementedException();
    }

    public void TryClean(Vector2 uv, Texture2D brush)
    {
        // throw new System.NotImplementedException();
    }

    public bool IsCleanable()
    {
        // throw new System.NotImplementedException();
        return true;
    }
}