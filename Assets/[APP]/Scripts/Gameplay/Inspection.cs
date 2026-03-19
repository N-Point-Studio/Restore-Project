using UnityEngine;
using DG.Tweening;
using System;

public class Inspection : MonoBehaviour
{
    public float rotateSpeed = 1f;
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

    public Transform assemblyRoot;
    private bool isContain = false;
    private bool isGameFinished = false;

    public Transform GetAssemblyRoot()
    {
        return assemblyRoot;
    }

    public void SetCenter(Vector3 center)
    {
        assemblyRoot.position = center;
    }

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

        GameplayUIManager.OnGameWrapped += HandleGameWrapped;
    }

    void OnDisable()
    {
        InteractionEvents.OnRotatePerformed -= OnRotatePerformed;
        InteractionEvents.OnZoomPerformed -= OnZoomPerformed;
        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
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

    private void HandleGameWrapped()
    {
        isGameFinished = true;
        FinishPosition();
    }

    public void OnRotatePerformed(Vector2 delta)
    {
        if (_mainCamera == null) return;

        float rotateY = -delta.x;
        float rotateX = delta.y;

        // axis dari camera
        Vector3 cameraRight = _mainCamera.transform.right;
        Vector3 cameraUp = _mainCamera.transform.up;

        // rotate horizontal (kiri kanan)
        transform.Rotate(cameraUp, rotateY, Space.World);

        // rotate vertical (atas bawah)
        transform.Rotate(cameraRight, rotateX, Space.World);
    }

    public void OnZoomPerformed(float zoomDelta)
    {
        if (!isContain || isGameFinished) return;
        if (_mainCamera == null) _mainCamera = Camera.main;

        Vector3 direction = (_targetPosition - _mainCamera.transform.position).normalized;

        float currentDistance = Vector3.Distance(
            _mainCamera.transform.position,
            _targetPosition
        );

        float targetDistance = currentDistance + (zoomDelta * zoomSpeed);
        targetDistance = Mathf.Clamp(targetDistance, minDistance, _initialDistance);
        _targetPosition = _mainCamera.transform.position + direction * targetDistance;
    }

    public void ResetPosition()
    {
        transform.SetPositionAndRotation(InitialInspectPosition, InitialInspectRotation);
        _targetPosition = InitialInspectPosition;
        _zoomVelocity = Vector3.zero;
    }

    public void FinishPosition()
    {
        isGameFinished = true;

        transform.DOKill();
        transform.DOMove(InitialInspectPosition, 1f).SetEase(Ease.OutBack);
        transform.DORotateQuaternion(InitialInspectRotation, 1f).SetEase(Ease.OutBack);

        _targetPosition = InitialInspectPosition;
        _zoomVelocity = Vector3.zero;
    }

    public void SetInspectionUsage(bool status)
    {
        isContain = status;
    }
}