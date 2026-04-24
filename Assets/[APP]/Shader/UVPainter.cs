using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class WorldSpacePainter : MonoBehaviour
{
    public Camera cam;

    [Tooltip("Masukkan Custom/MaskDecalShader ke sini")]
    public Shader paintingShader;
    public Shader maskShader;

    [Header("Brush Settings")]
    public Texture2D brushTexture;
    [Range(0, 1)] public float brushScale = 0.5f;
    [Range(0, 1)] public float brushStrength = 0.5f;
    public Color paintingColor = Color.white;
    public Transform cleaningTool;

    [Header("Material Integration")]
    [Tooltip("Nama properti mask di shader utama objekmu (misal: _BaseMap, _MaskTex, dll)")]
    public string maskTexturePropertyName = "_BaseMap";

    public Texture2D maskTexture;

    private RenderTexture paintingMap;
    private RenderTexture maskMap;
    private Material paintingMaterial;
    private Material maskMaterial;
    private Material paintableMaterial;
    private Renderer rend;
    private Mesh mesh;
    private CommandBuffer cb;

    [Header("Computeshader components (Debug)")]
    public ComputeShader cShader;
    ComputeBuffer cBuffer;
    int[] analysisResult;
    int kernalMain, kernalInit;
    public int maskPixel = 0;
    public int paintedPixel = 0;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;

        // 1. Setup material untuk proses "nge-cat" (menggunakan MaskDecalShader)
        paintingMaterial = new Material(paintingShader);
        paintableMaterial = rend.material;

        maskMaterial = new Material(maskShader);
        // maskMaterial.SetTexture("_BaseTexture", maskTexture);

        if (brushTexture != null)
            paintingMaterial.SetTexture("_BrushTexture", brushTexture);

        // 2. Buat RenderTexture sebagai "Kanvas" kosong berwarna hitam
        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        paintingMap.Create();

        maskMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        maskMap.Create();

        cb = new CommandBuffer();
        cb.name = "WorldSpacePainterCB";

        // 3. Bersihkan RenderTexture di awal
        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);

        cb.SetRenderTarget(maskMap);
        cb.ClearRenderTarget(true, true, Color.black);

        cb.DrawMesh(mesh, transform.localToWorldMatrix, maskMaterial, 0, 0);

        Graphics.ExecuteCommandBuffer(cb);
        cb.Clear();


        // Graphics.ExecuteCommandBuffer(cb);
        // cb.Clear();

        // 4. Pasang RenderTexture ini ke Material utama objek agar terlihat di game
        paintableMaterial.SetTexture(maskTexturePropertyName, paintingMap);
        maskMaterial.SetTexture("_MaskTexture", paintingMap);
        paintableMaterial.SetTexture("_Mask", maskTexture); // Pastikan shader utama juga bisa akses untuk blending
        paintingMaterial.SetTexture("_MaskTexture", maskTexture); // Pastikan shader cat juga bisa akses untuk blending
        maskPixel = AnalyzeTexture(); // Hitung total pixel pada mask di awal
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
                paintedPixel = AnalyseImage(); // Hitung pixel yang sudah dicat
                Debug.Log("Progress: " + paintedPixel + " / " + maskPixel);
            }
        }
    }

    int AnalyzeTexture()
    {
        kernalMain = cShader.FindKernel("CSMain");
        kernalInit = cShader.FindKernel("CSInit");
        cBuffer = new ComputeBuffer(1, sizeof(int));
        analysisResult = new int[1];

        cShader.SetTexture(kernalMain, "InputImage", maskMap);
        cShader.SetTexture(kernalInit, "InputImage", maskMap);
        cShader.SetBuffer(kernalMain, "ResultBuffer", cBuffer);
        cShader.SetBuffer(kernalInit, "ResultBuffer", cBuffer);

        cShader.Dispatch(kernalInit, 1, 1, 1);
        cShader.Dispatch(kernalMain, maskMap.width / 8, maskMap.height / 8, 1);

        cBuffer.GetData(analysisResult);

        cBuffer.Release();
        cBuffer = null;

        return analysisResult[0];
    }

    int AnalyseImage()
    {
        kernalMain = cShader.FindKernel("CSMain");
        kernalInit = cShader.FindKernel("CSInit");
        cBuffer = new ComputeBuffer(1, sizeof(int));
        analysisResult = new int[1];

        cShader.SetTexture(kernalMain, "InputImage", paintingMap);
        cShader.SetTexture(kernalInit, "InputImage", paintingMap);
        cShader.SetBuffer(kernalMain, "ResultBuffer", cBuffer);
        cShader.SetBuffer(kernalInit, "ResultBuffer", cBuffer);

        cShader.Dispatch(kernalInit, 1, 1, 1);
        cShader.Dispatch(kernalMain, paintingMap.width / 8, paintingMap.height / 8, 1);

        cBuffer.GetData(analysisResult);

        cBuffer.Release();
        cBuffer = null;

        return analysisResult[0];
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
        // cb.SetRenderTarget(maskMap);
        // cb.DrawMesh(mesh, transform.localToWorldMatrix, maskMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void OnDestroy()
    {
        // Jangan lupa bersihkan memory saat objek hancur
        if (paintingMap != null) { paintingMap.Release(); Destroy(paintingMap); }
        if (maskMap != null) { maskMap.Release(); Destroy(maskMap); }
        if (cb != null) cb.Release();
        if (paintingMaterial != null) Destroy(paintingMaterial);
        if (maskMaterial != null) Destroy(maskMaterial);
    }

    private void OnGUI()
    {
        // Tampilkan hasil RenderTexture di pojok kiri atas layar untuk Debugging
        // GUI.DrawTexture(new Rect(10, 10, 256, 256), maskMap, ScaleMode.ScaleToFit, false, 1);
    }
}