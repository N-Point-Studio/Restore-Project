using UnityEngine;

public class Inspection : MonoBehaviour
{
    public float rotateSpeed = 0.5f;
    public float zoomSpeed = 0.02f;
    public float smoothTime = 0.1f; // Semakin kecil semakin responsif

    [Header("Zoom Limits (Distance from Camera)")]
    public float minDistance = 2f;
    public float maxDistance = 10f;

    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private Vector3 _zoomVelocity = Vector3.zero;

    void Start()
    {
        _mainCamera = Camera.main;
        _targetPosition = transform.position;
    }

    void OnEnable()
    {
        InteractionEvents.OnRotatePerformed += OnRotatePerformed;
        InteractionEvents.OnZoomPerformed += OnZoomPerformed;
        // Reset target position saat aktif agar tidak ada lonjakan posisi
        _targetPosition = transform.position;
    }

    void OnDisable()
    {
        InteractionEvents.OnRotatePerformed -= OnRotatePerformed;
        InteractionEvents.OnZoomPerformed -= OnZoomPerformed;
    }

    void Update()
    {
        // Menggerakkan objek ke target position secara smooth
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _zoomVelocity,
            smoothTime
        );
    }

    public void OnRotatePerformed(Vector2 delta)
    {
        float rotateY = -delta.x * rotateSpeed;
        float rotateX = delta.y * rotateSpeed;

        transform.Rotate(Vector3.up, rotateY, Space.World);
        transform.Rotate(Vector3.right, rotateX, Space.World);
    }

    public void OnZoomPerformed(float zoomDelta)
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        Vector3 direction = (_targetPosition - _mainCamera.transform.position).normalized;

        float currentDistance = Vector3.Distance(_mainCamera.transform.position, _targetPosition);

        float targetDistance = currentDistance + (zoomDelta * zoomSpeed);

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        _targetPosition = _mainCamera.transform.position + direction * targetDistance;
    }
}