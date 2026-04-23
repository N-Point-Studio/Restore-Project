using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System;
using System.Collections.Generic;

public enum CursorState
{
    DefaultRounded,
    Crosshair,
    GrabClose,
    GrabOpen,
    Hover,
    Rotate
}

public enum CursorAlignment
{
    TopLeft,
    Center,
    Custom
}

[Serializable]
public struct CursorConfig
{
    public CursorState state;
    public Texture2D texture;
    
    [Header("Hotspot Settings")]
    public CursorAlignment alignment;
    
    public Vector2 customClickPosition; 

    public Vector2 GetHotspot()
    {
        if (texture == null) return Vector2.zero;
        
        switch (alignment)
        {
            case CursorAlignment.Center: return new Vector2(texture.width / 2f, texture.height / 2f);
            case CursorAlignment.TopLeft: return Vector2.zero;
            case CursorAlignment.Custom:
            default: return customClickPosition;
        }
    }
}

public class CursorController : MonoBehaviour
{
    public static CursorController instance;

    [Header("Cursor Configurations")]
    [SerializeField] private List<CursorConfig> cursorConfigs = new List<CursorConfig>();
    [SerializeField] private bool hideOnController = true;

    private PlayerInput playerInput;
    private bool usingController;
    private bool cursorVisible = true;
    
    private CursorState currentState = CursorState.DefaultRounded;
    private bool isCursorLocked = false; 

    private bool hasOverride = false;
    private CursorState overrideState;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null);
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
        SetCursorState(CursorState.DefaultRounded); 
        UpdateControllerStatus();
    }

    private void OnDestroy()
    {
        InputUser.onChange -= HandleOnInputChanged;
    }

    public void SetCursorState(CursorState newState)
    {
        if (isCursorLocked) return; 
        
        currentState = newState;
        UpdateCursorVisuals();
    }

    public void LockCursorState(CursorState lockedState)
    {
        isCursorLocked = true;
        currentState = lockedState;
        UpdateCursorVisuals();
    }

    public void UnlockCursorState()
    {
        isCursorLocked = false;
        UpdateCursorVisuals();
    }

    public void SetOverrideCursor(CursorState tempState)
    {
        hasOverride = true;
        overrideState = tempState;
        UpdateCursorVisuals();
    }

    public void ClearOverrideCursor()
    {
        hasOverride = false;
        UpdateCursorVisuals();
    }

    private void UpdateCursorVisuals()
    {
        CursorState stateToShow = hasOverride ? overrideState : currentState;
        
        int index = cursorConfigs.FindIndex(c => c.state == stateToShow);
        
        if (index >= 0)
        {
            CursorConfig config = cursorConfigs[index];
            if (config.texture != null)
            {
                Cursor.SetCursor(config.texture, config.GetHotspot(), CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
            }
        }
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
            if (PlayerInput.all.Count > 0) playerInput = PlayerInput.all[0];
            else return;
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
        if (cursorVisible != shouldBeVisible) SetCursorVisible(shouldBeVisible);
    }

    private void SetCursorVisible(bool visible)
    {
        cursorVisible = visible;
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}