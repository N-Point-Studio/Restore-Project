using DG.Tweening;
using Modules.SoundSystems;
using UnityEngine;

public abstract class DraggableTool : MonoBehaviour, IInteractObject, IDragObject, IDraggableTool
{
    [Header("Drag Settings")]
    [SerializeField] protected float raycastRange = 5f;
    [SerializeField] protected Transform grabPosition;
    [SerializeField] private Outline outline;
    [SerializeField] private ToolType toolType;


    [Header("Animation Settings")]
    [SerializeField] protected float returnAnimDuration = 0.5f;
    [SerializeField] protected float followMouseSpeed = 0.1f;
    [SerializeField] protected float surfaceMoveSpeed = 0.1f;
    [SerializeField] protected float surfaceRotateSpeed = 0.1f;

    [Header("Audio settings")]
    [SerializeField] protected AudioKey audioKey;
    [SerializeField] protected SoundType soundType;

    [Header("Animation Settings")]
    [SerializeField] protected Animator animator;

    protected bool isDragging { get; private set; }
    protected bool isReturning { get; private set; }

    protected Collider col;
    protected float grabOffset;
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected Vector3 targetPosition;
    protected Sequence returnSequence;

    private int currentAudioId = -1;

    protected virtual void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        col = GetComponent<Collider>();
        if (animator != null) animator.enabled = false;

        if (grabPosition != null)
        {
            grabOffset = Vector3.Distance(transform.position, grabPosition.position);
        }
    }

    private void OnEnable()
    {
        TutorialService.OnTutorialHighlightOn += HandleTutorialHighlightOn;
        TutorialService.OnTutorialHighlightOff += HandleTutorialHighlightOff;

    }

    private void OnDisable()
    {
        TutorialService.OnTutorialHighlightOn -= HandleTutorialHighlightOn;
        TutorialService.OnTutorialHighlightOff -= HandleTutorialHighlightOff;
    }

    private void HandleTutorialHighlightOn(ToolType toolType)
    {
        Debug.Log("Tutorial ON highlight: " + toolType);
        if (this.toolType == toolType) outline.enabled = true;
    }

    private void HandleTutorialHighlightOff(ToolType toolType)
    {
        Debug.Log("Tutorial OFF highlight: " + toolType);
        if (this.toolType == toolType) outline.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * raycastRange);
    }

    public void OnDragStarted(Vector3 worldPos)
    {
        isDragging = true;
        isReturning = false;

        SetColliderEnable(false);
        returnSequence?.Kill();

        targetPosition = CalculateTargetPosition(worldPos);
        // Debug.Log($"Drag {name} Started at {worldPos}");
        if (animator != null) animator.enabled = true;
    }

    public void OnDragPerformed(Vector3 worldPos)
    {
        if (!isDragging) return;

        targetPosition = CalculateTargetPosition(worldPos);

        transform.DOKill();
        transform.DOMove(targetPosition, followMouseSpeed).SetEase(Ease.OutQuad);
    }

    public void OnDragEnded(Vector3 worldPos)
    {
        ToolVFX(false);
        PlaySfx(false);
        isDragging = false;
        isReturning = true;
        if (animator != null) animator.enabled = false;

        SetColliderEnable(true);
        transform.DOKill();
        returnSequence?.Kill();

        returnSequence = DOTween.Sequence();
        returnSequence.Join(transform.DOMove(initialPosition, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.Join(transform.DORotate(initialRotation.eulerAngles, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.OnComplete(() => isReturning = false);
    }

    private Vector3 CalculateTargetPosition(Vector3 worldPos)
    {
        return new Vector3(worldPos.x, initialPosition.y, worldPos.z + grabOffset);
    }

    public void StickToSurface(Vector3 position, Quaternion rotation)
    {
        transform.DOKill();
        transform.DOMove(position, surfaceMoveSpeed).SetEase(Ease.OutQuad);
        transform.DORotate(rotation.eulerAngles, surfaceRotateSpeed).SetEase(Ease.OutQuad);
    }

    public void OnInteractDetected()
    {
    }

    public void OnInteractEnded()
    {
    }

    public void SetColliderEnable(bool isActive)
    {
        if (col != null)
        {
            col.enabled = isActive;
        }
    }

    public void PlaySfx(bool isPlaying)
    {
        if (soundType == SoundType.Once)
        {
            if (isPlaying)
            {
                SoundSystem.Instance.PlayAudio(audioKey, 1f, false, true, false);
            }
        }
        else
        {
            if (isPlaying)
            {
                bool isAlreadyPlaying = false;
                if (currentAudioId != -1)
                {
                    Audio activeAudio = SoundSystem.Instance.GetAudio(currentAudioId);
                    if (activeAudio != null && activeAudio.IsPlaying)
                    {
                        isAlreadyPlaying = true;
                    }
                }

                if (!isAlreadyPlaying)
                {
                    currentAudioId = SoundSystem.Instance.PlayAudio(audioKey, 1f, true, true, false);
                }
            }
            else
            {
                if (currentAudioId != -1)
                {
                    SoundSystem.Instance.StopAudio(currentAudioId);
                    currentAudioId = -1;
                }
            }
        }
    }

    public void PlayVfx(bool isPlaying) { ToolVFX(isPlaying); }
    protected abstract void ToolVFX(bool isPlaying);
}