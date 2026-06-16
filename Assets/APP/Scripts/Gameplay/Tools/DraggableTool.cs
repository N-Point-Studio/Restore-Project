using DG.Tweening;
using UnityEngine;

public class DraggableTool : MonoBehaviour, IInteractObject, IDragObject, IDraggableTool
{
    [Header("Drag Settings")]
    [SerializeField] private float raycastRange = 5f;
    [SerializeField] private Transform grabPosition;

    [Header("Animation Settings")]
    [SerializeField] protected float returnAnimDuration = 0.5f;
    [SerializeField] protected float followMouseSpeed = 0.1f;
    [SerializeField] protected float surfaceMoveSpeed = 0.1f;
    [SerializeField] protected float surfaceRotateSpeed = 0.1f;

    public bool isDragging { get; private set; }
    public bool isReturning { get; private set; }

    private Collider col;
    private float grabOffset;
    private Vector3 initialPosition;
    protected Quaternion initialRotation;
    private Vector3 targetPosition;
    protected Sequence returnSequence;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        col = GetComponent<Collider>();

        if (grabPosition != null)
        {
            grabOffset = Vector3.Distance(transform.position, grabPosition.position);
        }
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
        Debug.Log($"Drag {name} Started at {worldPos}");
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
        isDragging = false;
        isReturning = true;

        SetColliderEnable(true);
        Debug.Log($"Drag {name} Ended at {worldPos}");

        returnSequence?.Kill();
        returnSequence = DOTween.Sequence();
        returnSequence.Join(transform.DOMove(initialPosition, returnAnimDuration).SetEase(Ease.OutBack));
        returnSequence.Join(transform.DORotateQuaternion(initialRotation, returnAnimDuration).SetEase(Ease.OutBack));
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
        transform.DORotateQuaternion(rotation, surfaceRotateSpeed).SetEase(Ease.OutQuad);
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

    }

    public void PlayVfx(bool isPlaying)
    {

    }
}