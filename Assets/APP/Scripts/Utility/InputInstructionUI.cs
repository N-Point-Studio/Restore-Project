using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;
using UnityEngine.Localization;

public class InputInstructionUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected InputActionReference inputAction;
    [SerializeField] protected bool isHolding;
    
    [SerializeField] protected LocalizedString localizedInfo; 
    
    [SerializeField] protected bool customText;
    [SerializeField] protected bool hideEverythingIfNotController;
    [SerializeField] protected bool hideEverythingIfController;
    [SerializeField] protected bool hideEverythingIfMobile;
    [SerializeField] protected bool iconOnly;

    [Header("Component")]
    [SerializeField] protected TMP_Text textInput;
    [SerializeField] protected Image imageInput;
    [SerializeField] protected TMP_Text textInfo;
    [SerializeField] protected GamePadIconManager gamePadIconManager;
    [SerializeField] protected BetterContentSizeFitter contentSizeFitter;
    [SerializeField] protected HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] protected RectOffset defaultPadding;

    protected PlayerInput playerInput;
    protected bool usingController;
    public bool UsingController => usingController;

    protected Action<bool> onUsingController;
    
    private string currentTranslatedInfo = "";


    protected virtual void Awake()
    {
        if (defaultPadding != null && horizontalLayoutGroup != null)
        {
            defaultPadding = horizontalLayoutGroup.padding;
        }

        if (PlayerInput.all.Count > 0)
        {
            playerInput = PlayerInput.all[0];
        }

        InputUser.onChange += HandleOnInputChanged;

        SetupUI();
    }

    protected virtual void OnEnable()
    {
        if (localizedInfo != null)
        {
            localizedInfo.StringChanged += OnLocalizedStringChanged;
        }

        if (contentSizeFitter != null)
        {
            contentSizeFitter.RefreshContent();
        }
    }

    protected virtual void OnDisable()
    {
        if (localizedInfo != null)
        {
            localizedInfo.StringChanged -= OnLocalizedStringChanged;
        }
    }

    protected virtual void OnDestroy()
    {
        InputUser.onChange -= HandleOnInputChanged;
    }

    protected virtual void OnLocalizedStringChanged(string value)
    {
        currentTranslatedInfo = value;
        UpdateTextDisplay();
    }

    protected virtual void HandleOnInputChanged(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlsChanged)
        {
            SetupUI();
        }
    }

    protected virtual void OnValidate()
    {
        if (!Application.isPlaying)
        {
            SetupUI();

            if (contentSizeFitter != null)
            {
                contentSizeFitter.RefreshContent();
            }

            if (defaultPadding != null && horizontalLayoutGroup != null)
            {
                defaultPadding = horizontalLayoutGroup.padding;
            }
        }
    }

    public virtual void RegisterCallback(Action<bool> onUsingController)
    {
        this.onUsingController = onUsingController;
    }

    public virtual void ForceSetInputAction(InputActionReference action)
    {
        inputAction = action;
        SetupUI();
    }

    public virtual void ForceSetLocalizedText(bool isHolding, LocalizedString newLocalizedInfo)
    {
        this.isHolding = isHolding;
        
        if (localizedInfo != null) localizedInfo.StringChanged -= OnLocalizedStringChanged;
        
        localizedInfo = newLocalizedInfo;
        
        if (localizedInfo != null)
        {
            localizedInfo.StringChanged += OnLocalizedStringChanged;
            localizedInfo.RefreshString(); 
        }
        
        SetupUI();
    }

    public virtual void SetHideEverythingIfNotController(bool isHidden)
    {
        hideEverythingIfNotController = isHidden;
        SetupUI();
    }

    public virtual void SetHideEverythingIfController(bool isHidden)
    {
        hideEverythingIfController = isHidden;
        SetupUI();
    }

    // NEW: Setter for Mobile
    public virtual void SetHideEverythingIfMobile(bool isHidden)
    {
        hideEverythingIfMobile = isHidden;
        SetupUI();
    }

    public virtual void SetupUI()
    {
        if (inputAction == null)
            return;

        if (playerInput != null)
        {
            usingController = playerInput != null &&
                  playerInput.currentControlScheme != null &&
                  (playerInput.currentControlScheme.ToLower().Contains("gamepad") || playerInput.currentControlScheme.ToLower().Contains("controller"));

            if (!usingController && playerInput != null)
            {
                for (int i = 0; i < playerInput.devices.Count; i++)
                {
                    if (playerInput.devices[i] is Gamepad)
                    {
                        usingController = true;
                        break;
                    }
                }
            }
            onUsingController?.Invoke(usingController);
        }

        Image bgImage = null;
        if (TryGetComponent(out Image bg))
        {
            bgImage = bg;
        }

        if (bgImage != null) bgImage.enabled = true;
        if (textInfo != null) textInfo.gameObject.SetActive(true);
        if (horizontalLayoutGroup != null) horizontalLayoutGroup.gameObject.SetActive(true);

        UpdateTextDisplay();

        if (iconOnly)
        {
            if (textInfo != null) textInfo.gameObject.SetActive(false);
        }

        if (bgImage != null)
        {
            bgImage.enabled = !iconOnly;
        }

        string key;
        string device;

        if (usingController)
        {
            if (inputAction.action.bindings.Count > 1)
            {
                int bindingIndex = 1; 

                if (inputAction.action.expectedControlType.Equals(nameof(Vector2)))
                {
                    bindingIndex = -1;
                    for (int i = 0; i < inputAction.action.bindings.Count; i++)
                    {
                        var binding = inputAction.action.bindings[i];
                        if (binding.path.Contains("Gamepad") || binding.path.Contains("gamepad"))
                        {
                            bindingIndex = i;
                            break;
                        }
                    }

                    if (bindingIndex == -1)
                    {
                        bindingIndex = inputAction.action.bindings.Count > 1 ? 1 : 0;
                    }
                }

                if (bindingIndex >= inputAction.action.bindings.Count)
                {
                    bindingIndex = inputAction.action.bindings.Count - 1;
                }

                inputAction.action.GetBindingDisplayString(bindingIndex, out device, out key);

                if (key.Contains("/")) key = key.Split('/')[0];

                Sprite icon = gamePadIconManager != null ? gamePadIconManager.GetIcon(key, device, null) : null;

                if (horizontalLayoutGroup != null) horizontalLayoutGroup.gameObject.SetActive(icon != null);
                if (imageInput != null)
                {
                    imageInput.gameObject.SetActive(icon != null);
                    imageInput.sprite = icon;
                }
                if (textInput != null) textInput.gameObject.SetActive(false);
            }
            else
            {
                inputAction.action.GetBindingDisplayString(0, out device, out key);

                if (key.Contains("/")) key = key.Split('/')[0];

                Sprite icon = gamePadIconManager != null ? gamePadIconManager.GetIcon(key, device, null) : null;

                if (horizontalLayoutGroup != null) horizontalLayoutGroup.gameObject.SetActive(icon != null);
                if (imageInput != null)
                {
                    imageInput.gameObject.SetActive(icon != null);
                    imageInput.sprite = icon;
                }
                if (textInput != null) textInput.gameObject.SetActive(false);
            }

            if (hideEverythingIfController)
            {
                if (bgImage != null) bgImage.enabled = false;
                if (horizontalLayoutGroup != null) horizontalLayoutGroup.gameObject.SetActive(false);
                if (textInfo != null) textInfo.gameObject.SetActive(false);
            }
        }
        else 
        {
            if (textInput != null && inputAction.action != null)
            {
                inputAction.action.GetBindingDisplayString(0, out device, out key);
                Sprite icon = gamePadIconManager != null ? gamePadIconManager.GetIcon(key, device, null) : null;

                if (imageInput != null)
                {
                    imageInput.gameObject.SetActive(icon != null);
                    imageInput.sprite = icon;
                }

                textInput.gameObject.SetActive(icon == null);
                if (icon == null)
                {
                    textInput.text = inputAction.action.GetBindingDisplayString(0);
                    if (textInput.text.Contains("LEFT")) textInput.text = textInput.text.Replace("LEFT", "L");
                    if (textInput.text.Contains("RIGHT")) textInput.text = textInput.text.Replace("RIGHT", "R");
                    if (textInput.text.Contains("Control")) textInput.text = textInput.text.Replace("Control", "Ctrl");

                    textInput.gameObject.SetActive(true);
                }

                if (horizontalLayoutGroup != null)
                {
                    if (horizontalLayoutGroup.TryGetComponent(out Image hgImage)) hgImage.enabled = icon == null;
                    if (defaultPadding != null) horizontalLayoutGroup.padding = icon == null ? defaultPadding : new RectOffset(0, 0, 0, 0);
                }
            }

            if (hideEverythingIfNotController)
            {
                if (bgImage != null) bgImage.enabled = false;
                if (horizontalLayoutGroup != null) horizontalLayoutGroup.gameObject.SetActive(false);
                if (textInfo != null) textInfo.gameObject.SetActive(false);
            }
        }

        if (imageInput != null) imageInput.transform.parent.gameObject.SetActive(true);

        bool isMobile = Application.isMobilePlatform;

#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif

        if (hideEverythingIfMobile && isMobile)
        {
            if (horizontalLayoutGroup != null) horizontalLayoutGroup.gameObject.SetActive(true);
            if (textInfo != null) textInfo.gameObject.SetActive(true);

            if (imageInput != null)
            {
                imageInput.gameObject.SetActive(false);
                imageInput.transform.parent.gameObject.SetActive(false);
            }    
            if (textInput != null) textInput.gameObject.SetActive(false);
        }

        UpdateDoubleIcons();

        if (contentSizeFitter != null && gameObject.activeInHierarchy)
        {
            contentSizeFitter.RefreshContent();
        }
    }

    private void UpdateTextDisplay()
    {
        if (!customText && textInfo != null)
        {
            textInfo.text = $"{(isHolding ? $"(Hold) {currentTranslatedInfo}" : currentTranslatedInfo)}";
            textInfo.ForceMeshUpdate();
        }
        
        if (contentSizeFitter != null && gameObject.activeInHierarchy)
        {
            contentSizeFitter.RefreshContent();
        }
    }

    public virtual void UpdateDoubleIcons()
    {
    }
}