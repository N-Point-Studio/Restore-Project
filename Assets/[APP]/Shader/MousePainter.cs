using UnityEngine;
using UnityEngine.InputSystem;

public class MousePainter : MonoBehaviour
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
    private Mesh mesh; // Kita butuh data mesh-nya

    private void Start()
    {
        rend = GetComponent<Renderer>();
        // Ambil mesh dari MeshFilter (ganti ke SkinnedMeshRenderer jika pakai itu)
        mesh = GetComponent<MeshFilter>().sharedMesh;

        paintingMaterial = new Material(paintingShader);
        paintableMaterial = rend.material;

        paintingMap = new RenderTexture(1024, 1024, 0);
        paintingMap.Create();

        // Clear texture dengan warna hitam solid di awal
        RenderTexture.active = paintingMap;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        // Set RenderTexture ke shader utama (sesuaikan nama "_MainTex" atau "_BaseMap" di shader objekmu)
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

                // Set parameter jarak
                paintingMaterial.SetVector("_PaintPosition", hit.point);

                // --- INI YANG BARU ---
                // Kirim arah sorotan kamera ke shader
                paintingMaterial.SetVector("_PaintDirection", ray.direction);

                paintingMaterial.SetFloat("_Radius", brushRadius);
                paintingMaterial.SetFloat("_Hardness", brushHardness);

                RenderTexture.active = paintingMap;
                paintingMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
                RenderTexture.active = null;
            }
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    }
}