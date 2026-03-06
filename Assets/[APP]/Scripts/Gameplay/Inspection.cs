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
        GestureEvents.OnZoomPerformed += OnZoomPerformed;
        // Reset target position saat aktif agar tidak ada lonjakan posisi
        _targetPosition = transform.position;
    }

    void OnDisable()
    {
        InteractionEvents.OnRotatePerformed -= OnRotatePerformed;
        GestureEvents.OnZoomPerformed -= OnZoomPerformed;
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

        // 1. Dapatkan arah dari kamera ke objek
        Vector3 direction = (transform.position - _mainCamera.transform.position).normalized;

        // 2. Kalkulasi posisi target baru
        Vector3 nextTarget = _targetPosition + direction * (zoomDelta * zoomSpeed);

        // 3. Cek jarak target ke kamera (Clamping)
        float targetDistance = Vector3.Distance(_mainCamera.transform.position, nextTarget);

        if (targetDistance >= minDistance && targetDistance <= maxDistance)
        {
            _targetPosition = nextTarget;
        }
    }
}