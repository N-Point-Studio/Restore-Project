using UnityEngine;

public class Inspection : MonoBehaviour
{
    public float rotateSpeed = 0.5f;
    public float zoomSpeed = 0.02f;
    public float smoothTime = 0.1f;

    [Header("Zoom Limits")]
    public float minDistance = 2f;

    private float _initialDistance;

    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private Vector3 _zoomVelocity = Vector3.zero;

    private Vector3 InitialInspectPosition;
    private Quaternion InitialInspectRotation;

    void Start()
    {
        _mainCamera = Camera.main;
        _targetPosition = transform.position;
        InitialInspectPosition = transform.position;
        InitialInspectRotation = transform.rotation;
    }

    void OnEnable()
    {
        InteractionEvents.OnRotatePerformed += OnRotatePerformed;
        InteractionEvents.OnZoomPerformed += OnZoomPerformed;

        _mainCamera = Camera.main;

        _targetPosition = transform.position;

        _initialDistance = Vector3.Distance(
            _mainCamera.transform.position,
            transform.position
        );
    }

    void OnDisable()
    {
        InteractionEvents.OnRotatePerformed -= OnRotatePerformed;
        InteractionEvents.OnZoomPerformed -= OnZoomPerformed;
    }

    void Update()
    {
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

        float currentDistance = Vector3.Distance(
            _mainCamera.transform.position,
            _targetPosition
        );

        float targetDistance = currentDistance + (zoomDelta * zoomSpeed);

        // clamp antara zoom in dan posisi awal
        targetDistance = Mathf.Clamp(targetDistance, minDistance, _initialDistance);

        _targetPosition = _mainCamera.transform.position + direction * targetDistance;
    }

    public void ResetPosition()
    {
        transform.SetPositionAndRotation(InitialInspectPosition, InitialInspectRotation);

        _targetPosition = InitialInspectPosition;
        _zoomVelocity = Vector3.zero;
    }

}