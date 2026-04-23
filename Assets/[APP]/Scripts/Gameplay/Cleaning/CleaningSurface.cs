using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CleaningSurface : MonoBehaviour, ICleanSurface
{
    [Header("Shader Setting")]
    [SerializeField] private Shader paintingShader;
    [SerializeField] private Shader maskShader;
    private RenderTexture paintingMap;
    private RenderTexture maskMap;
    private Material paintingMaterial;
    private Material maskMaterial;
    private Material paintableMaterial;
    private Renderer rend;
    private Mesh mesh;
    private CommandBuffer cb;

    [Header("Mask Color Setting")]
    [SerializeField] private Color paintingColor = Color.white;

    [Header("Computeshader components")]
    [SerializeField] private ComputeShader progressShader;
    private ComputeBuffer cBuffer;
    private int[] analysisResult;
    private int kernelMain, kernelInit;
    public int maskPixel = 0;
    public int paintedPixel = 0;
    public float progress = 0;

    [Header("Material Integration")]
    [SerializeField] private string maskTexturePropertyName = "_SubMask";
    [SerializeField] private Texture2D maskTexture;

    public static event Action<ICleanSurface> OnCreated;

    void Start()
    {
        SetupShader();
        maskPixel = CalculateTexture(maskMap);
        OnCreated?.Invoke(this);
    }

    private void SetupShader()
    {
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;

        paintingMaterial = new Material(paintingShader);
        maskMaterial = new Material(maskShader);
        paintableMaterial = rend.material;

        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        paintingMap.Create();

        maskMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        maskMap.Create();

        cb = new CommandBuffer();
        cb.name = "SurfaceWorldPainter-" + name;

        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);

        cb.SetRenderTarget(maskMap);
        cb.ClearRenderTarget(true, true, Color.black);

        cb.DrawMesh(mesh, transform.localToWorldMatrix, maskMaterial, 0, 0);

        Graphics.ExecuteCommandBuffer(cb);
        cb.Clear();

        paintableMaterial.SetTexture(maskTexturePropertyName, paintingMap);
        maskMaterial.SetTexture("_MaskTexture", maskMap);
        paintableMaterial.SetTexture("_Mask", maskTexture);
    }

    int CalculateTexture(RenderTexture rt)
    {
        kernelMain = progressShader.FindKernel("CSMain");
        kernelInit = progressShader.FindKernel("CSInit");
        cBuffer = new ComputeBuffer(1, sizeof(int));
        analysisResult = new int[1];

        progressShader.SetTexture(kernelMain, "InputImage", rt);
        progressShader.SetTexture(kernelInit, "InputImage", rt);
        progressShader.SetBuffer(kernelMain, "ResultBuffer", cBuffer);
        progressShader.SetBuffer(kernelInit, "ResultBuffer", cBuffer);

        progressShader.Dispatch(kernelInit, 1, 1, 1);
        progressShader.Dispatch(kernelMain, rt.width / 8, rt.height / 8, 1);

        cBuffer.GetData(analysisResult);

        cBuffer.Release();
        cBuffer = null;

        return analysisResult[0];
    }

    public void CleanSurface(Vector3 hitPoint, Texture2D brush, Vector3 hitNormal, Vector3 direction, float scale = 0.5f, float strength = 0.5f)
    {
        paintingMaterial.SetTexture("_BrushTexture", brush);
        paintingMaterial.SetVector("_BrushPosition", hitPoint);
        paintingMaterial.SetVector("_PaintDirection", hitNormal);
        paintingMaterial.SetFloat("_BrushScale", scale);
        paintingMaterial.SetFloat("_BrushStrength", strength);
        paintingMaterial.SetColor("_PaintingColor", paintingColor);
        paintingMaterial.SetVector("_ToolDirection", direction);

        cb.Clear();
        cb.SetRenderTarget(paintingMap);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);

        paintedPixel = CalculateTexture(paintingMap);

        // progress = maskPixel == 0 ? 0 : (float)paintedPixel / (float)maskPixel * 100;
        // return progress;
    }

    public float GetCleaningProgress()
    {
        if (maskPixel == 0) return 0f;

        float currentProgress = ((float)paintedPixel / maskPixel) * 100f;

        // Kunci nilai dari 0 sampai 100 agar tidak tembus
        progress = Mathf.Clamp(currentProgress, 0f, 100f);

        return progress;
    }

    public bool IsCleanable()
    {
        var parent = GetComponentInParent<IClean>();
        if (parent != null) return parent.IsCleanable();
        return false;
    }

    private void OnDestroy()
    {
        if (paintingMap != null) paintingMap.Release();
        if (maskMap != null) maskMap.Release();
        if (paintingMaterial != null) Destroy(paintingMaterial);
        if (maskMaterial != null) Destroy(maskMaterial);
        cb?.Release();
    }

    public void ForceClean()
    {

    }

    public void OnInteractDetected()
    {
    }

    public void OnInteractEnded()
    {
    }

    public void SetColliderEnable(bool isActive)
    {
    }
}
