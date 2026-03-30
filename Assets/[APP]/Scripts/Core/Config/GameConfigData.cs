using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameConfigData), menuName = "App/Data/Game Config Data")]
public class GameConfigData : ScriptableObject
{
    [Header("Input & Game Feel")]
    public float scrollSensitivity = 10f;
    public float rotateSensitivity = 0.2f;
    public float dragThreshold = 25f;
    public float holdDelay = 0.2f;
    public float holdDuration = 0.5f;
    public float holdMoveTolerance = 5f;

    [Header("Artefact Assembly")]
    public float socketSnapDistance = 1f;
    public float assembleSnapDistance = 1.5f;
    public float recenterAnimDuration = 0.5f;

    [Header("Inspection Camera")]
    public float inspectionMinDistance = 2f;
    public float inspectionZoomSpeed = 0.02f;
    public float inspectionSmoothTime = 0.1f;
    public float inspectionResetDuration = 1f;

    [Header("System Settings")]
    public float autoSaveCooldown = 1.0f;
    public float minLoadingScreenDuration = 2.0f;
    public float bgmFadeDuration = 2.0f;
}