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

                Debug.Log("Hit UV: " + hit.textureCoord); // Ini untuk debugging, kita akan pakai hit.point
                // SET POSISI 3D DARI HIT POINT, BUKAN UV
                paintingMaterial.SetVector("_PaintPosition", hit.point);
                paintingMaterial.SetFloat("_Radius", brushRadius);
                paintingMaterial.SetFloat("_Hardness", brushHardness);

                // Aktifkan RenderTexture
                RenderTexture.active = paintingMap;
                paintingMaterial.SetPass(0);

                // --- INTI PERUBAHAN ---
                // Gambar langsung MESH dari objek ini. Vertex shader akan memipihkannya 
                // menjadi 2D di atas RenderTexture, namun Fragment shader tetap mengeksekusi radius 3D!
                Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);

                // Matikan aktif RenderTexture
                RenderTexture.active = null;
            }
        }
    }
}