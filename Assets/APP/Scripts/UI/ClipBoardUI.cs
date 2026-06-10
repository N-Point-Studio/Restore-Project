using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem; 

public class ClipBoardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private RectTransform animatedRect; 
    [SerializeField] private GameObject desktopControlsInfo;
    [SerializeField] private GameObject mobileControlsInfo;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 hoverPosition;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float stayDuration = 0.5f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private Vector2 startPosition;
    private Tween moveTween;
    private bool isHovering = false;
    private bool isClickedOpen = false; 
    
    private GameObject activeInfoPanel;

    private void Awake()
    {
        if (animatedRect == null) 
            animatedRect = GetComponent<RectTransform>();

        startPosition = animatedRect.anchoredPosition;
        ProgressBarUI.OnProgressBarCompleted += ProgressCompleted;

        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif

        if (isMobile)
        {
            activeInfoPanel = mobileControlsInfo;
            if (desktopControlsInfo != null) desktopControlsInfo.SetActive(false); // Hide Desktop forever
        }
        else
        {
            activeInfoPanel = desktopControlsInfo;
            if (mobileControlsInfo != null) mobileControlsInfo.SetActive(false); // Hide Mobile forever
        }
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
        ProgressBarUI.OnProgressBarCompleted -= ProgressCompleted;
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }

    private void ProgressCompleted()
    {
        if (isHovering || isClickedOpen) return; 

        moveTween?.Kill();

        Sequence seq = DOTween.Sequence();
        seq.Append(animatedRect.DOAnchorPos(hoverPosition, duration).SetEase(ease));
        seq.AppendInterval(stayDuration);
        seq.Append(animatedRect.DOAnchorPos(startPosition, duration).SetEase(ease));

        moveTween = seq;
    }

    private void MoveTo(Vector2 target)
    {
        moveTween?.Kill();
        moveTween = animatedRect.DOAnchorPos(target, duration).SetEase(ease);
    }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isClickedOpen)
        {
            isClickedOpen = true;
            MoveTo(hoverPosition);
            
            if (activeInfoPanel != null) activeInfoPanel.SetActive(true);
        }
    }

    private void Update()
    {
        if (isClickedOpen && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            
            Camera cam = null;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvas.worldCamera;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(animatedRect, screenPos, cam))
            {
                isClickedOpen = false;
                if (!isHovering) MoveTo(startPosition);
                if (activeInfoPanel != null) activeInfoPanel.SetActive(false);
            }
        }
    }
#else
    // Empty methods so the interfaces don't complain on PC builds
    public void OnPointerClick(PointerEventData eventData) { }
#endif

#if UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID)

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        if (!isClickedOpen) 
        {
            MoveTo(hoverPosition);
            if (activeInfoPanel != null) activeInfoPanel.SetActive(true);
        }
        
        CursorController.instance?.SetCursorState(CursorState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        if (!isClickedOpen) 
        {
            MoveTo(startPosition);
            if (activeInfoPanel != null) activeInfoPanel.SetActive(false);
        }
        
        CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
    }
#else
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }
#endif

}