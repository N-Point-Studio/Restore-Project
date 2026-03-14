using DG.Tweening;
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
    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        col = GetComponent<Collider>();
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

        returnSequence.Join(transform.DOMove(initialPosition, 0.5f).SetEase(Ease.OutBack));
        returnSequence.Join(transform.DORotateQuaternion(initialRotation, 0.5f).SetEase(Ease.OutBack));
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

    public SurfaceDetectionType ToolType => toolType;
}