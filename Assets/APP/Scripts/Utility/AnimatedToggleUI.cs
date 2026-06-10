using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Toggle))]
public class AnimatedToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The background image that changes color.")]
    [SerializeField] private Image backgroundCapsule;
    
    [Tooltip("The dot/knob that moves left and right.")]
    [SerializeField] private RectTransform handleKnob;

    [Tooltip("The text component inside the toggle.")]
    [SerializeField] private TMP_Text stateText;
    
    [Tooltip("The RectTransform of the text (so we can move it).")]
    [SerializeField] private RectTransform textRect;

    [Header("Localized Strings")]
    [SerializeField] private LocalizedString onStateString;
    [SerializeField] private LocalizedString offStateString;

    [Header("Color Settings")]
    [SerializeField] private Color onColor = new Color(0.85f, 0.8f, 0.4f); // Yellowish color from your image
    [SerializeField] private Color offColor = Color.white; // White color from your image

    [Header("Position Settings (Knob)")]
    [Tooltip("The X position of the knob when OFF.")]
    [SerializeField] private float knobOffPositionX = -30f;
    [Tooltip("The X position of the knob when ON.")]
    [SerializeField] private float knobOnPositionX = 30f;

    [Header("Position Settings (Text)")]
    [Tooltip("The X position of the text when OFF (usually opposite to the knob).")]
    [SerializeField] private float textOffPositionX = 20f;
    [Tooltip("The X position of the text when ON.")]
    [SerializeField] private float textOnPositionX = -20f;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;

    private Toggle myToggle;
    private string cachedOnText = "";
    private string cachedOffText = "";

    private void Awake()
    {
        myToggle = GetComponent<Toggle>();
        
        // Auto-assign textRect if not set
        if (textRect == null && stateText != null)
        {
            textRect = stateText.GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        // 1. Subscribe to Localization changes
        if (onStateString != null) onStateString.StringChanged += OnUpdateOnText;
        if (offStateString != null) offStateString.StringChanged += OnUpdateOffText;

        // 2. Listen to Toggle clicks
        myToggle.onValueChanged.AddListener(OnToggleStateChanged);
        
        // 3. Snap to initial state
        UpdateVisuals(myToggle.isOn, true);
    }

    private void OnDisable()
    {
        // 1. Unsubscribe to prevent memory leaks
        if (onStateString != null) onStateString.StringChanged -= OnUpdateOnText;
        if (offStateString != null) offStateString.StringChanged -= OnUpdateOffText;
        myToggle.onValueChanged.RemoveListener(OnToggleStateChanged);
        
        // 2. Kill Tweens
        if (handleKnob != null) handleKnob.DOKill();
        if (backgroundCapsule != null) backgroundCapsule.DOKill();
        if (textRect != null) textRect.DOKill();
    }

    // --- LOCALIZATION HANDLERS ---
    private void OnUpdateOnText(string translatedText)
    {
        cachedOnText = translatedText;
        if (myToggle.isOn && stateText != null) stateText.text = cachedOnText;
    }

    private void OnUpdateOffText(string translatedText)
    {
        cachedOffText = translatedText;
        if (!myToggle.isOn && stateText != null) stateText.text = cachedOffText;
    }

    // --- TOGGLE ANIMATION ---
    private void OnToggleStateChanged(bool isOn)
    {
        UpdateVisuals(isOn, false);
    }

    private void UpdateVisuals(bool isOn, bool isInstant)
    {
        float targetKnobX = isOn ? knobOnPositionX : knobOffPositionX;
        float targetTextX = isOn ? textOnPositionX : textOffPositionX;
        Color targetColor = isOn ? onColor : offColor;
        float duration = isInstant ? 0f : animationDuration;

        // 1. Update the Text Content
        if (stateText != null)
        {
            stateText.text = isOn ? cachedOnText : cachedOffText;
        }

        // 2. Animate Background Color
        if (backgroundCapsule != null)
        {
            backgroundCapsule.DOKill();
            backgroundCapsule.DOColor(targetColor, duration);
        }

        // 3. Animate Knob Position
        if (handleKnob != null)
        {
            handleKnob.DOKill();
            
            if (isInstant) handleKnob.anchoredPosition = new Vector2(targetKnobX, handleKnob.anchoredPosition.y);
            else handleKnob.DOAnchorPosX(targetKnobX, duration).SetEase(Ease.OutCirc);
        }

        // 4. Animate Text Position
        if (textRect != null)
        {
            textRect.DOKill();

            if (isInstant) textRect.anchoredPosition = new Vector2(targetTextX, textRect.anchoredPosition.y);
            else textRect.DOAnchorPosX(targetTextX, duration).SetEase(Ease.OutCirc);
        }
    }
}