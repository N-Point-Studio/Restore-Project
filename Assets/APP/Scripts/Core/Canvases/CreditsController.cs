using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class CreditsController : BaseMenuController, IPointerDownHandler, IPointerUpHandler
{
    [Header("Credits Settings")]
    [SerializeField] private RectTransform creditsContent;
    [Tooltip("How fast it scrolls normally")]
    [SerializeField] private float normalScrollSpeed = 50f;
    [Tooltip("How fast it scrolls when holding Space or touching the screen")]
    [SerializeField] private float fastScrollSpeed = 150f;
    
    [Header("Scroll Limits")]
    [Tooltip("The Y anchored position where the credits start (usually negative)")]
    [SerializeField] private float startingPositionY = -1000f; 
    [Tooltip("The Y anchored position where the credits stop scrolling")]
    [SerializeField] private float endPositionY = 2500f;

    [Header("UI Buttons")]
    [SerializeField] private Button buttonBackToMenu;
    [SerializeField] private Button buttonDiscord;
    [SerializeField] private Button buttonInstagram;

    private InputSystemService input;
    private GameConfigData configData;
    private bool isPointerDown = false;
    private bool isSpaceHeld = false;

    [Inject]
    public void Construct(InputSystemService input, GameConfigData configData)
    {
        this.input = input;
        this.configData = configData;
    }

    protected override void Awake()
    {
        base.Awake();
        
        // Button Subscriptions
        if (buttonBackToMenu != null) buttonBackToMenu.onClick.AddListener(CloseCredits);
        if (buttonDiscord != null) buttonDiscord.onClick.AddListener(OpenDiscord);
        if (buttonInstagram != null) buttonInstagram.onClick.AddListener(OpenInstagram);

        // Input Subscriptions
        if (input != null)
        {
            input.OnUIKeycodeSpaceStarted += HandleSpaceStarted;
            input.OnUIKeycodeSpaceCanceled += HandleSpaceCanceled;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Button Unsubscriptions
        if (buttonBackToMenu != null) buttonBackToMenu.onClick.RemoveListener(CloseCredits);
        if (buttonDiscord != null) buttonDiscord.onClick.RemoveListener(OpenDiscord);
        if (buttonInstagram != null) buttonInstagram.onClick.RemoveListener(OpenInstagram);

        // Input Unsubscriptions
        if (input != null)
        {
            input.OnUIKeycodeSpaceStarted -= HandleSpaceStarted;
            input.OnUIKeycodeSpaceCanceled -= HandleSpaceCanceled;
        }
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);
        if (isActive)
        {
            isPointerDown = false;
            isSpaceHeld = false;

            if (creditsContent != null)
            {
                creditsContent.anchoredPosition = new Vector2(creditsContent.anchoredPosition.x, startingPositionY);
            }
        }
    }

    private void HandleSpaceStarted() => isSpaceHeld = true;
    private void HandleSpaceCanceled() => isSpaceHeld = false;

    private void Update()
    {
        if (!IsActive || creditsContent == null) return;

        if (creditsContent.anchoredPosition.y >= endPositionY)
        {
            creditsContent.anchoredPosition = new Vector2(creditsContent.anchoredPosition.x, endPositionY);
            return; 
        }

        bool isFastForwarding = isSpaceHeld || isPointerDown;
        float currentSpeed = isFastForwarding ? fastScrollSpeed : normalScrollSpeed;

        creditsContent.anchoredPosition += Vector2.up * (currentSpeed * Time.unscaledDeltaTime);
    }

    // --- MOBILE TOUCH DETECTORS ---
    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
    }

    // --- BUTTON ACTIONS ---
    private void OpenDiscord()
    {
        if (configData == null)
            return;

        if (!string.IsNullOrEmpty(configData.discordURL))
        {
            Application.OpenURL(configData.discordURL);
        }
    }

    private void OpenInstagram()
    { 
        if (configData == null)
            return;
            
        if (!string.IsNullOrEmpty(configData.instagramURL))
        {
            Application.OpenURL(configData.instagramURL);
        }
    }

    private void CloseCredits()
    {
        SetActive(false);
        MainMenuEvents.TriggerCloseCredits();
    }
}