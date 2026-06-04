using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Toggle))]
public class AnimatedToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The background image that changes color.")]
    [SerializeField] private Image backgroundCapsule;
    
    [Tooltip("The dot/knob that moves left and right.")]
    [SerializeField] private RectTransform handleKnob;

    [Header("Color Settings")]
    [SerializeField] private Color onColor = new Color(0.2f, 0.8f, 0.3f); // iOS Green
    [SerializeField] private Color offColor = new Color(0.8f, 0.8f, 0.8f); // Gray

    [Header("Position Settings")]
    [Tooltip("The X position of the dot when OFF.")]
    [SerializeField] private float offPositionX = -20f;
    
    [Tooltip("The X position of the dot when ON.")]
    [SerializeField] private float onPositionX = 20f;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;

    private Toggle myToggle;

    private void Awake()
    {
        myToggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        myToggle.onValueChanged.AddListener(OnToggleStateChanged);
        UpdateVisuals(myToggle.isOn, true);
    }

    private void OnDisable()
    {
        myToggle.onValueChanged.RemoveListener(OnToggleStateChanged);
        
        if (handleKnob != null) handleKnob.DOKill();
        if (backgroundCapsule != null) backgroundCapsule.DOKill();
    }

    private void OnToggleStateChanged(bool isOn)
    {
        UpdateVisuals(isOn, false);
    }

    private void UpdateVisuals(bool isOn, bool isInstant)
    {
        float targetX = isOn ? onPositionX : offPositionX;
        Color targetColor = isOn ? onColor : offColor;
        float duration = isInstant ? 0f : animationDuration;

        if (backgroundCapsule != null)
        {
            backgroundCapsule.DOKill();
            backgroundCapsule.DOColor(targetColor, duration);
        }

        if (handleKnob != null)
        {
            handleKnob.DOKill();
            
            if (isInstant)
            {
                handleKnob.anchoredPosition = new Vector2(targetX, handleKnob.anchoredPosition.y);
            }
            else
            {
                handleKnob.DOAnchorPosX(targetX, duration).SetEase(Ease.OutBack);
            }
        }
    }
}