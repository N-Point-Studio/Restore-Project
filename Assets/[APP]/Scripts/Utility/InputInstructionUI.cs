using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class InputInstructionUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected InputActionReference inputAction;
    [SerializeField] protected bool isHolding;
    [SerializeField] protected string info;
    [SerializeField] protected bool customText;
    [SerializeField] protected bool hideEverythingIfNotController;
    [SerializeField] protected bool hideEverythingIfController;
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

    protected virtual void OnDestroy()
    {
        InputUser.onChange -= HandleOnInputChanged;
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

    protected virtual void OnEnable()
    {
        if (contentSizeFitter != null)
        {
            contentSizeFitter.RefreshContent();
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

    public virtual void ForceSetText(bool isHolding, string text)
    {
        this.isHolding = isHolding;
        info = text;
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

        string key;
        string device;

        Image bgImage = null;
        if (TryGetComponent(out Image bg))
        {
            bgImage = bg;
        }

        if (bgImage != null)
        {
            bgImage.enabled = true;
        }

        textInfo.gameObject.SetActive(true);

        if (horizontalLayoutGroup != null) // Text Input & Image Input Panel 
        {
            horizontalLayoutGroup.gameObject.SetActive(true);
        }

        if (!customText && textInfo != null)
        {
            textInfo.text = $"{(isHolding ? $"(Hold) {info}" : info)}";
        }

        if (iconOnly)
        {
            textInfo.gameObject.SetActive(false);
        }

        if (bgImage != null)
        {
            bgImage.enabled = !iconOnly;
        }

        if (usingController)
        {
            if (inputAction.action.bindings.Count > 1)
            {
                int bindingIndex = 1; // Default binding index for non-Vector2 inputs

                if (inputAction.action.expectedControlType.Equals(nameof(Vector2)))
                {
                    // Find the first gamepad binding for Vector2 inputs
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

                    // Fallback to index 1 if no gamepad binding found
                    if (bindingIndex == -1)
                    {
                        bindingIndex = inputAction.action.bindings.Count > 1 ? 1 : 0;
                    }
                }

                // Ensure binding index is within bounds
                if (bindingIndex >= inputAction.action.bindings.Count)
                {
                    bindingIndex = inputAction.action.bindings.Count - 1;
                }

                inputAction.action.GetBindingDisplayString(bindingIndex, out device, out key);

                if (key.Contains("/"))
                {
                    key = key.Split('/')[0];
                }

                Sprite icon = gamePadIconManager.GetIcon(key, device, null);

                // Additional debug logging if icon is null
                if (icon == null && inputAction.action.expectedControlType.Equals(nameof(Vector2)))
                {
                    Debug.LogWarning($"[InputInstructionUI] No icon found for Vector2 input - Key: '{key}', Device: '{device}'");
                }

                if (horizontalLayoutGroup != null)
                {
                    horizontalLayoutGroup.gameObject.SetActive(icon != null);
                }

                imageInput.gameObject.SetActive(icon != null);
                imageInput.sprite = icon;

                textInput.gameObject.SetActive(false);
            }
            else
            {
                // Handle case when there's only one binding
                inputAction.action.GetBindingDisplayString(0, out device, out key);

                if (key.Contains("/"))
                {
                    key = key.Split('/')[0];
                }

                Sprite icon = gamePadIconManager.GetIcon(key, device, null);

                if (horizontalLayoutGroup != null)
                {
                    horizontalLayoutGroup.gameObject.SetActive(icon != null);
                }

                imageInput.gameObject.SetActive(icon != null);
                imageInput.sprite = icon;

                textInput.gameObject.SetActive(false);
            }

            if (hideEverythingIfController)
            {
                if (bgImage != null)
                {
                    bgImage.enabled = false;
                }

                if (horizontalLayoutGroup != null)
                {
                    horizontalLayoutGroup.gameObject.SetActive(false);
                }

                textInfo.gameObject.SetActive(false);
            }
        }
        else
        {
            if (textInput != null)
            {
                inputAction.action.GetBindingDisplayString(0, out device, out key);
                Sprite icon = gamePadIconManager.GetIcon(key, device, null);

                imageInput.gameObject.SetActive(icon != null);
                imageInput.sprite = icon;

                textInput.gameObject.SetActive(icon == null);
                if (icon == null)
                {
                    textInput.text = inputAction.action.GetBindingDisplayString(0);

                    // shorten the text
                    if (textInput.text.Contains("LEFT"))
                        textInput.text = textInput.text.Replace("LEFT", "L");
                    if (textInput.text.Contains("RIGHT"))
                        textInput.text = textInput.text.Replace("RIGHT", "R");
                    if (textInput.text.Contains("Control"))
                        textInput.text = textInput.text.Replace("Control", "Ctrl");

                    textInput.gameObject.SetActive(true);
                }

                if (horizontalLayoutGroup != null)
                {
                    horizontalLayoutGroup.GetComponent<Image>().enabled = icon == null;

                    if (defaultPadding != null)
                        horizontalLayoutGroup.padding = icon == null ? defaultPadding : new RectOffset(0, 0, 0, 0);
                }
            }

            if (hideEverythingIfNotController)
            {
                if (bgImage != null)
                {
                    bgImage.enabled = false;
                }

                if (horizontalLayoutGroup != null)
                {
                    horizontalLayoutGroup.gameObject.SetActive(false);
                }

                textInfo.gameObject.SetActive(false);
            }
        }

        UpdateDoubleIcons();

        if (contentSizeFitter != null && gameObject.activeInHierarchy)
        {
            contentSizeFitter.RefreshContent();
        }
    }

    public virtual void UpdateDoubleIcons()
    {

    }
}