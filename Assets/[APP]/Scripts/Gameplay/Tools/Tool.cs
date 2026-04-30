using DG.Tweening;
using Modules.SoundSystems;
using UnityEngine;

public abstract class Tool : MonoBehaviour, IInteractObject, IToolObject, IPressObject
{
    [Header("Animation Settings")]
    [SerializeField] protected float returnAnimDuration = 0.5f;
    [SerializeField] protected float followMouseSpeed = 0.1f;
    [SerializeField] protected float SurfaceMoveSpeed = 0.1f;
    [SerializeField] protected float SurfaceRotateSpeed = 0.1f;

    [Header("Initial Placement")]
    [SerializeField] protected ToolOrigin origin;
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected Collider col;
    protected Sequence returnSequence;
    protected bool isReturning;
    protected bool isUsed;
    protected bool isStickingToSurface;
    protected bool isSfxOn = false;
    private int currentAudioId = -1;

    [Header("Audio settings")]
    [SerializeField] protected AudioKey audioKey;
    [SerializeField] protected SoundType soundType;

    [Header("Animation Settings")]
    [SerializeField] protected Animator animator;
    private Vector3 movement;

    protected virtual void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        col = GetComponent<Collider>();
        animator.enabled = false;
        movement = transform.position;
    }
    public void OnPressStarted() => PressStarted();
    public void OnPressEnded() => PressEnded();

    public void OnInteractDetected() => InteractDetected();
    public void OnInteractEnded() => InteractEnded();
    public bool IsStickingToSurface => IsStickingToSurface;


    public AudioKey ToolSFX => audioKey;
    public IInteractObject GetOrigin() => origin;


    public void SetColliderEnable(bool isActive)
    {
        col.enabled = isActive;
    }

    public void Use()
    {
        origin.SetCollider(true);
        returnSequence?.Kill();
        col.enabled = false;
        isUsed = true;
        animator.enabled = true;
    }

    public void Return()
    {
        col.enabled = true;
        isReturning = true;
        isUsed = false;
        animator.enabled = false;

        returnSequence?.Kill();
        returnSequence = DOTween.Sequence();

        returnSequence.Join(transform.DOMove(initialPosition, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.Join(transform.DORotateQuaternion(initialRotation, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.OnComplete(() => isReturning = false);
        origin.SetCollider(false);
    }

    public void FollowMouse(Vector3 worldPos)
    {
        if (isReturning) return;
        transform.DOKill();
        transform.DOMove(worldPos, followMouseSpeed).SetEase(Ease.OutQuad);
        // Moving();
    }

    public void StickToSurface(Vector3 position, Quaternion rotation)
    {
        var pos = position;

        transform.DOKill();
        transform.DOMove(position, SurfaceMoveSpeed).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(rotation, SurfaceRotateSpeed).SetEase(Ease.OutQuad);

        if (movement != pos)
        {

        }
        //StickSurface
    }

    public void SetStickToSurface(bool isSticking)
    {
        isStickingToSurface = isSticking;
    }

    protected abstract void InteractDetected();
    protected abstract void InteractEnded();
    protected abstract void PressStarted();
    protected abstract void PressEnded();
    protected abstract void ToolVFX(bool isPlaying);
    protected abstract void Moving();
    protected abstract void StickSurface(float x, float y);

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
}