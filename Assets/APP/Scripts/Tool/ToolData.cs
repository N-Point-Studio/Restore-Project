using Modules.SoundSystems;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = nameof(ToolData), menuName = "App/Data/Tool Data")]
public class ToolData : ScriptableObject
{
    [SerializeField] private BaseData baseData;
    public BaseData BaseData => baseData;

    [SerializeField] private ToolType toolType;
    public ToolType ToolType => toolType;


    [Header("Localization Data")]
    [SerializeField] private LocalizedString localizedItemName;
    public LocalizedString LocalizedItemName => localizedItemName;

    [Header("Audio Settings")]
    [SerializeField] private AudioKey customToolSFX;
    public AudioKey CustomToolSFX => customToolSFX;
    [SerializeField] private SoundType soundType;
    public SoundType SoundType => soundType;

    [Header("Animation Settings")]
    [SerializeField] private float returnAnimationDuration;
    public float ReturnAnimationDuration => returnAnimationDuration;
    [SerializeField] private float followMouseSpeed;
    public float FollowMouseSpeed => followMouseSpeed;
    [SerializeField] private float surfaceMoveSpeed;
    public float SurfaceMoveSpeed => surfaceMoveSpeed;
    [SerializeField] private float surfaceRotateSpeed;
    public float SurfaceRotateSpeed => surfaceRotateSpeed;

    [Header("Spawn Settings")]
    [SerializeField] private CustomTransform spawnTransform;
    public CustomTransform SpawnTransform => spawnTransform;
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [Header("Brush Settings (Optioinal)")]
    [SerializeField] private BrushData brushData;
    public BrushData BrushData => brushData;

    [Header("Animation Settings (optional)")]
    [SerializeField] private float animationSmoothSpeed = 10f;
    public float AnimationSmoothSpeed => animationSmoothSpeed;
}

[System.Serializable]
public class BrushData
{
    [SerializeField] private Texture2D brushTexture;
    public Texture2D BrushTexture => brushTexture;
    [Range(0, 1)][SerializeField] private float brushScale = 0f;
    public float BrushScale => brushScale;
    [Range(0, 1)][SerializeField] private float brushStrength = 0f;
    public float BrushStrength => brushStrength;
    [Range(0, 1)][SerializeField] private float brushDepth = 0f;
    public float BrushDepth => brushDepth;
    [SerializeField] private Color brushColor = Color.white;
    public Color BrushColor => brushColor;
}