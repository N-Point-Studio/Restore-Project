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
    
    private int currentAudioId = -1;

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
        transform.DOKill();
        transform.DOMove(worldPos, 0.2f).SetEase(Ease.OutQuad);
    }
    
    public void StickToSurface(Vector3 position, Quaternion rotation)
    {
        transform.DOKill();
        transform.DOMove(position, 0.3f).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(rotation, 0.3f).SetEase(Ease.OutQuad);
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
                SoundSystem.Instance.PlayAudio(audioKey, 1f, false, true, false);
                vfx.Play();
            }
        }
        else // Mode Continuous / Looping
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
                
                vfx.Play();
            }
            else
            {
                if (currentAudioId != -1)
                {
                    SoundSystem.Instance.StopAudio(currentAudioId);
                    currentAudioId = -1;
                }
                
                vfx.Stop();
            }
        }
    }

    public void PlayVfx(bool isPlaying) { }

    public void Moving(bool isMoving)
    {
        this.isMoving = isMoving;
    }

    public SurfaceDetectionType ToolType => toolType;
}