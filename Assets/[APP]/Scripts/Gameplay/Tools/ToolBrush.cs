using Modules;
using UnityEngine;

public class ToolBrush : Tool, IBrushTool
{
    [Header("Brush Setting (Optional)")]
    [SerializeField] private Texture2D brushTexture;
    [Range(0, 1)][SerializeField] private float brushScale = 0.5f;
    [Range(0, 1)][SerializeField] private float brushStrength = 0.5f;
    [SerializeField] private Color paintingColor = Color.white;

    [Header("Animation Smoothing")]
    [SerializeField] private float animationSmoothSpeed = 10f;
    private float currentMoveX = 0f;
    private float currentMoveY = 0f;
    private float targetMoveX = 0f;
    private float targetMoveY = 0f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem smokeVFX;
    [SerializeField] private ParticleSystem dustVFX;
    private Vector3 lastPosition;

    private readonly int MoveHorizontal = Animator.StringToHash("MoveHorizontal");
    private readonly int MoveVertical = Animator.StringToHash("MoveVertical");

    protected override void Awake()
    {
        base.Awake();
        SetVfxEmission(0f, 0f);
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPosition;

        Vector2 movement = new Vector2(delta.x, delta.y);

        if (movement.magnitude < 0.0001f)
        {
            movement = Vector2.zero;
        }
        else
        {
            movement = movement.normalized;
        }

        targetMoveX = movement.x;
        targetMoveY = movement.y;

        lastPosition = transform.position;

        if (animator != null && animator.isActiveAndEnabled)
        {
            currentMoveX = Mathf.Lerp(currentMoveX, targetMoveX, Time.deltaTime * animationSmoothSpeed);
            currentMoveY = Mathf.Lerp(currentMoveY, targetMoveY, Time.deltaTime * animationSmoothSpeed);

            animator.SetFloat(MoveHorizontal, currentMoveX);
            animator.SetFloat(MoveVertical, currentMoveY);
        }
    }

    protected override void InteractDetected() { }
    protected override void InteractEnded() { }
    protected override void PressEnded() { }
    protected override void PressStarted() { }

    private void SetVfxEmission(float smokeRate, float dustRate)
    {
        if (smokeVFX != null)
        {
            var smokeEmission = smokeVFX.emission;
            smokeEmission.rateOverTime = smokeRate;
        }

        if (dustVFX != null)
        {
            var dustEmission = dustVFX.emission;
            dustEmission.rateOverTime = dustRate;
        }
    }

    protected override void ToolVFX(bool isPlaying)
    {
        if (isPlaying)
        {
            SetVfxEmission(2f, 8f);
        }
        else
        {
            SetVfxEmission(0f, 0f);
        }
    }

    public Texture2D GetBrush => brushTexture;
    public float BrushScale => brushScale;
    public float BrushStrength => brushStrength;
    public Color BrushColor => paintingColor;
    public Transform BrushTransform => transform;
}