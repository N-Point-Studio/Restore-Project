using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// [CreateAssetMenu(fileName = "GamePad Icon Manager", menuName = "New GamePad Icon Manager")]
public class GamePadIconManager : ScriptableObject
{
    [SerializeField] private MouseKeyboardIcons mouseKeyboard;
    [SerializeField] private GamepadIcons xbox;
    [SerializeField] private GamepadIcons nintendo;
    [SerializeField] private GamepadIcons ps4;

    public Sprite GetIcon(string displayString, string deviceLayoutName, string controlPath)
    {
        if (string.IsNullOrEmpty(deviceLayoutName))
            return null;

        // Handle null or empty display strings
        if (string.IsNullOrEmpty(displayString))
            return null;

        // Check if this is a mouse/keyboard device
        bool isMouseKeyboard = InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Mouse") || 
                              InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard");

        // Map display names to internal control paths
        string mappedPath = displayString switch
        {
            "RS/Up" => "rightStick/up",
            "RS/Down" => "rightStick/down",
            "RS/Left" => "rightStick/left",
            "RS/Right" => "rightStick/right",
            "LS/Up" => "leftStick/up",
            "LS/Down" => "leftStick/down",
            "LS/Left" => "leftStick/left",
            "LS/Right" => "leftStick/right",
            // Handle common Vector2 binding variations
            "rightStick" => "rightStick",
            "leftStick" => "leftStick",
            "Right Stick" => "rightStick",
            "Left Stick" => "leftStick",
            // Only normalize to lowercase for gamepad inputs, preserve case for mouse/keyboard
            _ => isMouseKeyboard ? displayString : displayString.ToLower()
        };

        Sprite icon = default;
        if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
            icon = ps4.GetSprite(mappedPath);
        else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "SwitchProControllerHID"))
            icon = nintendo.GetSprite(mappedPath);
        else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "XInputController"))
            icon = xbox.GetSprite(mappedPath);
        else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Mouse"))
            icon = mouseKeyboard.GetSprite(mappedPath);

        return icon;
    }

    [Serializable]
    public struct MouseKeyboardIcons
    {
        public Sprite mouseLeftClick;
        public Sprite mouseRightClick;
        public Sprite mouseMiddleClick;
        public Sprite mouseScoll;

        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "leftButton": return mouseLeftClick;
                case "rightButton": return mouseRightClick;
                case "middleButton": return mouseMiddleClick;
                case "scroll/y": return mouseScoll;
            }
            return null;
        }
    }

    [Serializable]
    public struct GamepadIcons
    {
        public Sprite buttonSouth;
        public Sprite buttonNorth;
        public Sprite buttonEast;
        public Sprite buttonWest;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite rightStick;
        public Sprite leftStickPress;
        public Sprite rightStickPress;

        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            // Normalize to lowercase for consistent matching
            string normalizedPath = controlPath?.ToLower() ?? "";
            
            switch (normalizedPath)
            {
                case "buttonsouth": return buttonSouth;
                case "buttonnorth": return buttonNorth;
                case "buttoneast": return buttonEast;
                case "buttonwest": return buttonWest;
                case "start": return startButton;
                case "select": return selectButton;
                case "lefttrigger": return leftTrigger;
                case "righttrigger": return rightTrigger;
                case "leftshoulder": return leftShoulder;
                case "rightshoulder": return rightShoulder;
                case "dpad": return dpad;
                case "dpad/up": return dpadUp;
                case "dpad/down": return dpadDown;
                case "dpad/left": return dpadLeft;
                case "dpad/right": return dpadRight;
                // Left stick variations
                case "ls/up":
                case "leftstick/up":
                case "leftstick": return leftStick;
                case "ls/down":
                case "leftstick/down": return leftStick;
                case "ls/left":
                case "leftstick/left": return leftStick;
                case "ls/right":
                case "leftstick/right": return leftStick;
                // Right stick variations
                case "rs/up":
                case "rightstick/up":
                case "rs/down":
                case "rightstick/down":
                case "rs/left":
                case "rightstick/left":
                case "rs/right":
                case "rightstick/right":
                case "rightstick": return rightStick;
                case "leftstickpress": return leftStickPress;
                case "rightstickpress": return rightStickPress;
            }
            return null;
        }
    }
}
