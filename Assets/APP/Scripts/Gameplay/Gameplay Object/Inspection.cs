using UnityEngine;
using DG.Tweening;
using VContainer;

public class Inspection : MonoBehaviour
{
    public float rotateSpeed = 1f;

    private float _initialDistance;

    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private Vector3 _zoomVelocity = Vector3.zero;

    private Vector3 InitialInspectPosition;
    private Quaternion InitialInspectRotation;
    private Vector3 InitialInspectScale;

    public Transform assemblyRoot;
    private bool isContain = false;
    private bool isGameFinished = false;
    private bool isAssembling = false;

    private TutorialService tutorialService;
    private GameConfigData config;
    private GameplayManager gameplayManager;

    private PlaneReference planeReference;

    [SerializeField] private Renderer sphereRenderer;

    [Inject]
    public void Construct(TutorialService tutorialService, GameConfigData config, GameplayManager gameplayManager, PlaneReference plane)
    {
        this.tutorialService = tutorialService;
        this.config = config;
        this.gameplayManager = gameplayManager;
        this.planeReference = plane;
        this._zoomVelocity = config.inspectionZoomSpeed * Vector3.one;

        Debug.Log("zoomvelocity: " + _zoomVelocity);
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _targetPosition = transform.position;
        InitialInspectPosition = transform.position;
        InitialInspectRotation = transform.rotation;
        InitialInspectScale = transform.localScale;
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

    void OnDestroy()
    {
        InteractionEvents.OnRotatePerformed -= OnRotatePerformed;
        InteractionEvents.OnZoomPerformed -= OnZoomPerformed;

        AssembleEvents.OnAssemblePerformed -= HandleAssemblePerformed;
        AssembleEvents.OnAssembleFinished -= HandleAssembleFinished;

        GameplayUIManager.OnGameWrapped -= HandleGameWrapped;
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
        // transform.position = Vector3.SmoothDamp(
        //     transform.position,
        //     _targetPosition,
        //     ref _zoomVelocity,
        //     config.inspectionSmoothTime
        // );
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _zoomVelocity,
            config.inspectionSmoothTime,
            config.inspectionZoomSpeed // Parameter batas kecepatan masuk di sini
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

        //rotate tutorial
        if (!tutorialService.IsProcessing && tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 2)
        {
            tutorialService.CompleteAndAdvance(true);
            tutorialService.TriggerHighlight(true, ToolType.Chisel);
        }
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

        // 1. Tambahkan variabel ini untuk meredam respons input scroll (Coba angka 0.05f atau 0.1f)
        float zoomStepSpeed = 0.05f;

        // 2. Kalikan zoomDelta dengan zoomStepSpeed, BUKAN zoomValue
        float targetDistance = currentDistance + (zoomDelta * zoomStepSpeed);

        // 3. zoomValue tetap murni dipakai untuk batas kedekatan (agar tidak tembus pandang)
        targetDistance = Mathf.Clamp(targetDistance, gameplayManager.zoomValue, _initialDistance);

        _targetPosition = _mainCamera.transform.position + direction * targetDistance;

        planeReference.SetReferencePosition(_targetPosition);

        //zoom tutorial
        if (!tutorialService.IsProcessing && tutorialService.CurrentStage == 0 && tutorialService.CurrentModule == 1)
        {
            tutorialService.CompleteAndAdvance(true);
        }
    }

    public void ResetPosition()
    {
        transform.SetPositionAndRotation(InitialInspectPosition, InitialInspectRotation);
        transform.localScale = InitialInspectScale;
        _targetPosition = InitialInspectPosition;
        planeReference.SetReferencePosition(_targetPosition);
        _zoomVelocity = Vector3.zero;
    }

    public void RotateHorizontally(float deltaX)
    {
        if (isGameFinished)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            Vector3 cameraUp = _mainCamera != null ? _mainCamera.transform.up : Vector3.up;
            float rotateY = -deltaX * rotateSpeed * 0.2f;
            transform.Rotate(cameraUp, rotateY, Space.World);
        }
    }

    public void FinishPosition()
    {
        isGameFinished = true;

        transform.DOKill();
        transform.DOMove(InitialInspectPosition, config.inspectionResetDuration).SetEase(Ease.OutBack);
        transform.DORotateQuaternion(Quaternion.Euler(gameplayManager.finalRotation), config.inspectionResetDuration).SetEase(Ease.OutBack);
        transform.DOScale(InitialInspectScale * gameplayManager.finalScaleMultiplier, config.inspectionResetDuration).SetEase(Ease.OutBack);

        _targetPosition = InitialInspectPosition;
        _zoomVelocity = Vector3.zero;
    }

    public void SetSphereRenderer(bool isActive)
    {
        if (sphereRenderer == null) return;
        sphereRenderer.enabled = isActive;
    }

    public void SetInspectionUsage(bool status) => isContain = status;
    private void HandleAssemblePerformed() => isAssembling = true;
    private void HandleAssembleFinished() => isAssembling = false;

}