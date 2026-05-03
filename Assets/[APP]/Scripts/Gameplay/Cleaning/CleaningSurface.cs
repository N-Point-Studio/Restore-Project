using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CleaningSurface : MonoBehaviour, ICleanSurface, IInteractObject, IDragObject
{
    [Header("Shader Setting")]
    [SerializeField] private Shader paintingShader;
    [SerializeField] private Shader maskShader;
    [SerializeField] private bool isFullShader = false;
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
    public bool isDirtRemoved = false;

    [Header("Material Integration")]
    [SerializeField] private string maskTexturePropertyName = "_SubMask";
    [SerializeField] private Texture2D maskTexture;

    public static event Action<ICleanSurface> OnCreated;

    private IDragObject parentDragObject;

    private void Awake()
    {
        if (transform.parent != null)
        {
            parentDragObject = transform.parent.GetComponentInParent<IDragObject>();
        }
    }

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

        paintingMap = CreateRenderTexture(1024);
        maskMap = CreateRenderTexture(1024);

        if (!isFullShader)
        {
            maskMaterial.SetTexture("_MaskTexture", maskTexture);
            paintingMaterial.SetTexture("_MaskTexture", maskTexture);
            paintableMaterial.SetTexture("_Mask", maskTexture);
        }

        cb = new CommandBuffer { name = $"SurfaceWorldPainter-{name}" };

        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);

        cb.SetRenderTarget(maskMap);
        cb.ClearRenderTarget(true, true, Color.black);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, maskMaterial, 0, 0);

        Graphics.ExecuteCommandBuffer(cb);
        cb.Clear();

        paintableMaterial.SetTexture(maskTexturePropertyName, paintingMap);
        if (isFullShader)
        {
            paintableMaterial.SetTexture("_Mask", maskMap);
            paintingMaterial.SetTexture("_MaskTexture", maskMap);
        }
    }

    private RenderTexture CreateRenderTexture(int size)
    {
        RenderTexture rt = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point
        };
        rt.Create();
        return rt;
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
        var tempPercentage = paintedPixel;

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
        isDirtRemoved = paintedPixel > tempPercentage;
    }

    public float GetCleaningProgress()
    {
        if (maskPixel == 0) return 0f;
        float currentProgress = ((float)paintedPixel / maskPixel) * 100f;
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
        if (cb != null && paintingMap != null)
        {
            cb.Clear();
            cb.SetRenderTarget(paintingMap);
            cb.ClearRenderTarget(true, true, paintingColor);
            Graphics.ExecuteCommandBuffer(cb);
            paintableMaterial.SetFloat("_DirtStrength", 0f);

            paintedPixel = maskPixel;
            progress = 100f;
        }
    }

    public void OnInteractDetected() { }
    public void OnInteractEnded() { }
    public void SetColliderEnable(bool isActive) { }

    public void OnDragStarted(Vector3 worldPos) => parentDragObject?.OnDragStarted(worldPos);
    public void OnDragPerformed(Vector3 worldPos) => parentDragObject?.OnDragPerformed(worldPos);
    public void OnDragEnded(Vector3 worldPos) => parentDragObject?.OnDragEnded(worldPos);

    public void ShowClue(bool isShowing)
    {
        if (isShowing)
        {
            paintableMaterial.SetFloat("_ClueStrength", 1);
            // paintableMaterial.SetFloat("_BlackoutStrength", 0.2f);
        }
        else
        {
            paintableMaterial.SetFloat("_ClueStrength", 0);
            // paintableMaterial.SetFloat("_BlackoutStrength", 1f);
        }
    }

    public bool IsDustRemoved()
    {
        return isDirtRemoved;
    }

    // private void OnGUI()
    // {
    //     GUI.DrawTexture(new Rect(10, 10, 256, 256), maskMap, ScaleMode.ScaleToFit, false, 1);
    //     GUI.DrawTexture(new Rect(10, 400, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    // }
}