using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System;

public class CursorController : MonoBehaviour
{
    public static CursorController instance;

    [SerializeField] private Texture2D cursorTextureDefault;
    [SerializeField] private Vector2 clickPosition = Vector2.zero;
    [SerializeField] private bool hideOnController = true;

    private PlayerInput playerInput;
    private bool usingController;
    private bool cursorVisible = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InputUser.onChange += HandleOnInputChanged;
    }

    private void Start()
    {
        Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
        UpdateControllerStatus();
    }

    private void OnDestroy()
    {
        InputUser.onChange -= HandleOnInputChanged;
    }

    private void HandleOnInputChanged(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged || 
            change == InputUserChange.DevicePaired || 
            change == InputUserChange.DeviceUnpaired)
        {
            UpdateControllerStatus();
        }
    }

    private void UpdateControllerStatus()
    {
        if (playerInput == null)
        {
            if (PlayerInput.all.Count > 0)
                playerInput = PlayerInput.all[0];
            else
                return;
        }

        usingController = false;

        if (!string.IsNullOrEmpty(playerInput.currentControlScheme))
        {
            string scheme = playerInput.currentControlScheme;
            if (scheme.IndexOf("gamepad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                scheme.IndexOf("controller", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                usingController = true;
            }
        }

        if (!usingController)
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

        UpdateCursorVisibility();
    }

    private void UpdateCursorVisibility()
    {
        bool shouldBeVisible = !(hideOnController && usingController);
        
        if (cursorVisible != shouldBeVisible)
        {
            SetCursorVisible(shouldBeVisible);
        }
    }

    private void SetCursorVisible(bool visible)
    {
        cursorVisible = visible;
        Cursor.visible = visible;
        
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}