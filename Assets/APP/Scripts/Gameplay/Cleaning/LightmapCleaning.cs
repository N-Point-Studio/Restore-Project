using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class LightmapCleaning : MonoBehaviour
{
    public Camera cam;

    [Header("Brush Settings")]
    [SerializeField][Range(0f, 1f)] private float brushRadius = 0.1f;
    [SerializeField][Range(0f, 1f)] private float brushHardness = 0.5f;

    [Header("Mask Settings")]
    [SerializeField] private Shader paintingShader;
    [SerializeField] private Color color = Color.red;

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

        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        paintingMap.Create();

        cb = new CommandBuffer();
        cb.name = "WorldSpacePainter";

        // Bersihkan render texture dengan warna hitam di awal
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

                // Panggil ReceivePaint dengan parameter yang benar
                ReceivePaint(hit.point, hit.normal, brushRadius, brushHardness);
            }
        }
    }

    public void ReceivePaint(Vector3 hitPoint, Vector3 hitNormal, float radius, float hardness)
    {
        paintingMaterial.SetVector("_PaintPositionWS", hitPoint);
        // HAPUS tanda minus (-) pada hitNormal. Kita butuh arah normal yang asli.
        paintingMaterial.SetVector("_PaintNormalWS", hitNormal);
        paintingMaterial.SetVector("_PaintColor", color);
        paintingMaterial.SetFloat("_SpreadRadius", radius);

        cb.Clear();
        // SANGAT PENTING: Gunakan Load & Store agar frame coretan sebelumnya tidak terhapus (Accumulation)
        cb.SetRenderTarget(paintingMap, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void OnDestroy()
    {
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

    private void OnGUI()
    {
        // Debug Render Texture
        GUI.DrawTexture(new Rect(10, 10, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    }
}