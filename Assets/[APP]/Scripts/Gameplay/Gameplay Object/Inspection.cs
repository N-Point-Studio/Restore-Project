using UnityEngine;
using DG.Tweening;
using NINESOFT.TUTORIAL_SYSTEM;
using VContainer;
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

    private TutorialService tutorialService;

    private bool isAssembling = false;

    [Inject]
    public void Construct(TutorialService tutorialService)
    {
        this.tutorialService = tutorialService;
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

        AssembleEvents.OnAssemblePerformed += HandleAssemblePerformed;
        AssembleEvents.OnAssembleFinished += HandleAssembleFinished;

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

        AssembleEvents.OnAssemblePerformed -= HandleAssemblePerformed;
        AssembleEvents.OnAssembleFinished -= HandleAssembleFinished;

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

    public Transform GetAssemblyRoot() => assemblyRoot;

    private void HandleGameWrapped()
    {
        isGameFinished = true;
        FinishPosition();
    }

    public void OnRotatePerformed(Vector2 delta)
    {
        if (!isContain || isAssembling) return;
        if (_mainCamera == null) return;

        float rotateY = -delta.x;
        float rotateX = delta.y;

        // Axis from camera
        Vector3 cameraRight = _mainCamera.transform.right;
        Vector3 cameraUp = _mainCamera.transform.up;

        // Rotate horizontal (left right)
        transform.Rotate(cameraUp, rotateY, Space.World);

        // Rotate vertical (up down)
        transform.Rotate(cameraRight, rotateX, Space.World);

        tutorialService.CompleteTutorial(TutorialIDs.ROTATE_INSPECT, 0, 2);
        tutorialService.StartTutorial(TutorialIDs.BRUSH_DUST, 0, 3);
    }

    public void OnZoomPerformed(float zoomDelta)
    {
        if (!isContain || isGameFinished || isAssembling) return;
        if (_mainCamera == null) _mainCamera = Camera.main;

        Vector3 direction = (_targetPosition - _mainCamera.transform.position).normalized;

        float currentDistance = Vector3.Distance(
            _mainCamera.transform.position,
            _targetPosition
        );

        float targetDistance = currentDistance + (zoomDelta * zoomSpeed);
        targetDistance = Mathf.Clamp(targetDistance, minDistance, _initialDistance);
        _targetPosition = _mainCamera.transform.position + direction * targetDistance;

        // Complete zoom tutorial
        tutorialService.CompleteTutorial(TutorialIDs.ZOOM_INSPECT, 0, 1);
        tutorialService.StartTutorial(TutorialIDs.ROTATE_INSPECT, 0, 2);
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

    public void SetInspectionUsage(bool status) => isContain = status;
    private void HandleAssemblePerformed() => isAssembling = true;
    private void HandleAssembleFinished() => isAssembling = false;

}