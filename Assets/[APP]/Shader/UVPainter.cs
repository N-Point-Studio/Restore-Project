using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class WorldSpacePainter : MonoBehaviour
{
    public Camera cam;

    [Tooltip("Masukkan Custom/MaskDecalShader ke sini")]
    public Shader paintingShader;

    [Header("Brush Settings")]
    public Texture2D brushTexture;
    [Range(0, 1)] public float brushScale = 0.5f;
    [Range(0, 1)] public float brushStrength = 0.5f;
    public Color paintingColor = Color.white;
    public Transform cleaningTool;

    [Header("Material Integration")]
    [Tooltip("Nama properti mask di shader utama objekmu (misal: _BaseMap, _MaskTex, dll)")]
    public string maskTexturePropertyName = "_BaseMap";

    private RenderTexture paintingMap;
    private Material paintingMaterial;
    private Material paintableMaterial;
    private Renderer rend;
    private Mesh mesh;
    private CommandBuffer cb;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;

        // 1. Setup material untuk proses "nge-cat" (menggunakan MaskDecalShader)
        paintingMaterial = new Material(paintingShader);
        paintableMaterial = rend.material;

        if (brushTexture != null)
            paintingMaterial.SetTexture("_BrushTexture", brushTexture);

        // 2. Buat RenderTexture sebagai "Kanvas" kosong berwarna hitam
        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        paintingMap.Create();

        cb = new CommandBuffer();
        cb.name = "WorldSpacePainterCB";

        // 3. Bersihkan RenderTexture di awal
        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);
        Graphics.ExecuteCommandBuffer(cb);
        cb.Clear();

        // 4. Pasang RenderTexture ini ke Material utama objek agar terlihat di game
        paintableMaterial.SetTexture(maskTexturePropertyName, paintingMap);
    }

    private void Update()
    {
        // Pastikan mouse terdeteksi (New Input System)
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Jangan nge-cat objek lain yang menghalangi
                if (hit.collider.gameObject != gameObject) return;
                Vector3 toolUpDir = cleaningTool != null ? cleaningTool.up : Vector3.up;

                // Kirim titik sentuh dan arah normal permukaan ke fungsi cat
                ReceivePaint(hit.point, hit.normal, toolUpDir);
            }
        }
    }

    public void ReceivePaint(Vector3 hitPoint, Vector3 hitNormal, Vector3 toolDir)
    {
        // Variabel ini HARUS SAMA PERSIS dengan yang ada di MaskDecalShader
        paintingMaterial.SetVector("_BrushPosition", hitPoint);
        paintingMaterial.SetVector("_PaintDirection", hitNormal);

        paintingMaterial.SetFloat("_BrushScale", brushScale);
        paintingMaterial.SetFloat("_BrushStrength", brushStrength);
        paintingMaterial.SetColor("_PaintingColor", paintingColor);

        paintingMaterial.SetVector("_ToolDirection", toolDir);

        // Eksekusi proses pengecatan ke dalam RenderTexture
        cb.Clear();
        cb.SetRenderTarget(paintingMap);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void OnDestroy()
    {
        // Jangan lupa bersihkan memory saat objek hancur
        if (paintingMap != null) { paintingMap.Release(); Destroy(paintingMap); }
        if (cb != null) cb.Release();
        if (paintingMaterial != null) Destroy(paintingMaterial);
    }

    private void OnGUI()
    {
        // Tampilkan hasil RenderTexture di pojok kiri atas layar untuk Debugging
        GUI.DrawTexture(new Rect(10, 10, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    }
}