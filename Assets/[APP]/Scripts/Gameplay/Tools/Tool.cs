using DG.Tweening;
using UnityEngine;

public abstract class Tool : MonoBehaviour, IInteractObject, IToolObject, IPressObject
{

    [Header("Tool type")]
    // [SerializeField] protected SurfaceDetectionType toolType;

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

    protected virtual void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        col = GetComponent<Collider>();
    }
    public void OnPressStarted() => PressStarted();
    public void OnPressEnded() => PressEnded();

    public void OnInteractDetected() => InteractDetected();
    public void OnInteractEnded() => InteractEnded();
    public bool IsStickingToSurface => IsStickingToSurface;
    public IInteractObject GetOrigin() => origin as IInteractObject;


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
    }

    public void Return()
    {
        col.enabled = true;
        isReturning = true;
        isUsed = false;

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
    }

    public void StickToSurface(Vector3 position, Quaternion rotation)
    {
        transform.DOKill();
        transform.DOMove(position, SurfaceMoveSpeed).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(rotation, SurfaceRotateSpeed).SetEase(Ease.OutQuad);
    }

    public void SetStickToSurface(bool isSticking)
    {
        isStickingToSurface = isSticking;
    }

    protected abstract void InteractDetected();
    protected abstract void InteractEnded();
    protected abstract void PressStarted();
    protected abstract void PressEnded();
}