using DG.Tweening;
using Modules;
using Modules.SoundSystems;
using UnityEngine;

public class ToolObject : MonoBehaviour, IInteractObject, ITool, IPressObject
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Collider col;
    private Sequence returnSequence;
    private bool isReturning;
    [SerializeField] private Texture2D brush;
    [SerializeField] private SurfaceDetectionType toolType;

    [Header("Animation Settings")]
    [SerializeField] private float returnAnimDuration = 0.5f;
    [SerializeField] private ParticleSystem vfx;

    [Header("Audio Variations")]
    [SerializeField] private AudioKey audioKey;
    [SerializeField] private SoundType soundType;

    private bool isMoving = false;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        col = GetComponent<Collider>();
        vfx.Stop();
    }

    public void OnInteractDetected() { }
    public void OnInteractEnded() { }

    public void Use()
    {
        returnSequence?.Kill();
        col.enabled = false;
    }

    public void Return()
    {
        col.enabled = true;
        isReturning = true;

        returnSequence?.Kill();
        returnSequence = DOTween.Sequence();

        returnSequence.Join(transform.DOMove(initialPosition, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.Join(transform.DORotateQuaternion(initialRotation, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.OnComplete(() => isReturning = false);
    }

    public void FollowMouse(Vector3 worldPos)
    {
        if (isReturning) return;
        transform.position = worldPos;
    }
    public void StickToSurface(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }

    public void OnPressEnded() { }
    public void OnPressStarted() { }

    public Texture2D GetBrush()
    {
        return brush;
    }

    public void SetColliderEnable(bool isActive)
    {
        col.enabled = isActive;
    }

    public void PlaySfx(bool isPlaying)
    {
        if (soundType == SoundType.Once)
        {
            if (isPlaying)
            {
                AudioEvents.TriggerPlayCustomSFX(audioKey);
                vfx.Play();

            }
        }
        else
        {
            AudioEvents.TriggerPlayContinuousSFX(audioKey, isPlaying);
            if (isPlaying)
            {
                vfx.Play();
            }
            else if (!isPlaying)
            {
                vfx.Stop();
            }
        }
    }

    public void PlayVfx(bool isPlaying)
    {
    }

    public void Moving(bool isMoving)
    {
        this.isMoving = isMoving;
    }

    public SurfaceDetectionType ToolType => toolType;
}