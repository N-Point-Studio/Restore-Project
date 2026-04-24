using Modules;
using UnityEngine;

public class ToolBrush : Tool, IBrushTool
{
    [Header("Brush Setting (Optional)")]
    [SerializeField] private Texture2D brushTexture;
    [Range(0, 1)][SerializeField] private float brushScale = 0.5f;
    [Range(0, 1)][SerializeField] private float brushStrength = 0.5f;
    [SerializeField] private Color paintingColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void InteractDetected() { }
    protected override void InteractEnded() { }
    protected override void PressEnded() { }
    protected override void PressStarted() { }

    public Texture2D GetBrush => brushTexture;
    public float BrushScale => brushScale;
    public float BrushStrength => brushStrength;
    public Color BrushColor => paintingColor;
    public Transform BrushTransform => transform;
}
