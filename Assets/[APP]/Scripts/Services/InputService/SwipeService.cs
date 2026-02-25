using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SwipeService : IInitializable, IDisposable
{
    private readonly PressService pressService;
    private readonly InputService input;
    private const float DragThreshold = 25f;

    private Vector2 startPos;
    private Vector2 lastPos;
    private bool isSwiping;
    private bool isPrimaryHold;
    private bool isSecondaryHold;

    public event Action OnInteractionStart;
    public event Action OnInteractionEnd;
    public event Action<Vector3> OnDragPerformed;
    public event Action<Vector2> OnRotatePerformed;

    [Inject]
    public SwipeService(PressService pressService, InputService input)
    {
        this.pressService = pressService;
        this.input = input;
    }

    public void Initialize()
    {
        pressService.OnPressStarted += OnPrimaryStarted;
        pressService.OnPressEnded += OnPrimaryEnded;
        input.OnPressMoved += OnPressMoved;

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
        input.OnSecondaryPressStarted += OnSecondaryPressStarted;
        input.OnSecondaryPressEnded += OnSecondaryPressEnded;
#endif
    }

    public void Dispose()
    {
        pressService.OnPressStarted -= OnPrimaryStarted;
        pressService.OnPressEnded -= OnPrimaryEnded;
        input.OnPressMoved -= OnPressMoved;

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
        input.OnSecondaryPressStarted -= OnSecondaryPressStarted;
        input.OnSecondaryPressEnded -= OnSecondaryPressEnded;
#endif
    }

    #region Handlers
    private void OnPrimaryStarted(Vector2 pos) => StartInteraction(pos, true);
    private void OnPrimaryEnded(Vector2 pos) => EndInteraction(ref isPrimaryHold);

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
    private void OnSecondaryPressStarted(Vector2 pos) => StartInteraction(pos, false);
    private void OnSecondaryPressEnded(Vector2 pos) => EndInteraction(ref isSecondaryHold);
#endif

    private void StartInteraction(Vector2 pos, bool isPrimary)
    {
        if (isPrimary) isPrimaryHold = true;
        else isSecondaryHold = true;

        startPos = pos;
        lastPos = pos;
        isSwiping = false;
        OnInteractionStart?.Invoke();
    }

    private void EndInteraction(ref bool holdFlag)
    {
        holdFlag = false;
        if (isSwiping) OnInteractionEnd?.Invoke();
        isSwiping = false;
    }

    private void OnPressMoved(Vector2 currentPos)
    {
        if (!isPrimaryHold && !isSecondaryHold) return;

        if (!isSwiping && Vector2.Distance(startPos, currentPos) > DragThreshold)
            isSwiping = true;

        if (!isSwiping) return;

        Vector2 delta = currentPos - lastPos;
        lastPos = currentPos;

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
        if (isSecondaryHold)
            OnRotatePerformed?.Invoke(delta);
        else if (isPrimaryHold)
            OnDragPerformed?.Invoke(new Vector3(currentPos.x, currentPos.y, 0));
#else
        OnRotatePerformed?.Invoke(delta);
        OnDragPerformed?.Invoke(new Vector3(currentPos.x, currentPos.y, 0));
#endif
    }
    #endregion
}