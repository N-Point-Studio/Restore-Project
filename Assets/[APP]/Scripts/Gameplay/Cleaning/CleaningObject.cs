using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CleaningObject : MonoBehaviour
{
    public Camera cam;
    public Shader paintingShader;

    [Header("Brush Settings")]
    [Tooltip("Masukkan objek sikat/kain lap 3D kamu ke sini!")]
    public Transform cleaningTool;

    public Texture2D brushTexture;
    [Range(0.01f, 2.0f)] public float brushSize = 0.5f;
    [Range(0.01f, 1.0f)] public float paintStrength = 0.1f;

    private RenderTexture paintingMap;
    private Material paintingMaterial;
    private Material paintableMaterial;
    private Renderer rend;
    private Mesh mesh;
    private CommandBuffer cb;
    private RaycastHit hit;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;

        paintingMaterial = new Material(paintingShader);
        paintableMaterial = rend.material;

        if (brushTexture != null)
            paintingMaterial.SetTexture("_BrushTexture", brushTexture);

        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        paintingMap.Create();

        cb = new CommandBuffer();
        cb.name = "WorldSpacePainter";

        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);
        Graphics.ExecuteCommandBuffer(cb);
        cb.Clear();

        paintableMaterial.SetTexture("_SubMask", paintingMap);
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

                paintingMaterial.SetVector("_CameraPosition", cam.transform.position);

                // --- LOGIKA ROTASI SIKAT ---
                // Ambil arah "Atas" dari objek sikat 3D. 
                // Jika kamu belum pasang objek sikat di Inspector, default ke atas dunia.
                Vector3 toolUpDir = cleaningTool != null ? cleaningTool.up : Vector3.up;

                // Kirim data ke fungsi ReceivePaint
                ReceivePaint(hit.point, hit.normal, brushSize, 0.5f, toolUpDir);
            }
        }
    }

    // Parameter terakhir diubah dari float (sudut) menjadi Vector3 (arah atas alat)
    public void ReceivePaint(Vector3 hitPoint, Vector3 hitNormal, float radius, float hardness, Vector3 toolUp)
    {
        paintingMaterial.SetVector("_PaintPosition", hitPoint);
        paintingMaterial.SetVector("_PaintDirection", -hitNormal);

        paintingMaterial.SetFloat("_Radius", radius);
        paintingMaterial.SetFloat("_Hardness", hardness);
        paintingMaterial.SetFloat("_Strength", paintStrength);

        // Kirim arah rotasi alat ke Shader
        paintingMaterial.SetVector("_ToolUp", toolUp);

        cb.Clear();
        cb.SetRenderTarget(paintingMap);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void OnDestroy()
    {
        if (paintingMap != null) { paintingMap.Release(); Destroy(paintingMap); }
        if (cb != null) cb.Release();
        if (paintingMaterial != null) Destroy(paintingMaterial);
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    }
}