using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainUIController : BaseMenuController
{
    [Header("Desktop")]
    [SerializeField] private ButtonInputInstructionUI buttonWrapUp;

    [Header("Mobile")]
    [SerializeField] private ButtonInputInstructionUI buttonMobileWrapUp;
    [SerializeField] private Toggle hintToggle;
    [SerializeField] private Button buttonPause;

    private InputSystemService input;
    private bool canWrapUp;
    
    private ButtonInputInstructionUI activeWrapUpButton; 
    
    public event Action OnWrapUp;
    public event Action OnPauseRequest;

    [Inject]
    public void Construct(InputSystemService input)
    {
        this.input = input;
        this.input.OnPlayerKeycodeEnterPerformed += OnPlayerKeycodeEnterPerformed;

        InteractionEvents.OnTabPerformed += SyncHintToggleOn;
        InteractionEvents.OnTabCanceled += SyncHintToggleOff;
    }

    protected override void Awake()
    {
        base.Awake();
        
        bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        isMobile = true;
#endif

        if (isMobile)
        {
            activeWrapUpButton = buttonMobileWrapUp;
            if (buttonWrapUp != null && buttonWrapUp.transform.parent != null)
                buttonWrapUp.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            activeWrapUpButton = buttonWrapUp;
            if (buttonMobileWrapUp != null)
                buttonMobileWrapUp.gameObject.SetActive(false);
        }

        if (activeWrapUpButton != null) 
            activeWrapUpButton.OnClick += OnWrapUpClick;

        if (hintToggle != null) 
        {
            hintToggle.onValueChanged.AddListener(OnHintToggleValueChanged);
            hintToggle.gameObject.SetActive(false); 
        }

        if (buttonPause != null)
        {
            buttonPause.onClick.AddListener(OnPauseClicked);
            buttonPause.gameObject.SetActive(isMobile);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Unsubscribe from the active button
        if (activeWrapUpButton != null) 
            activeWrapUpButton.OnClick -= OnWrapUpClick;
            
        if (hintToggle != null) 
            hintToggle.onValueChanged.RemoveListener(OnHintToggleValueChanged);

        if (buttonPause != null)
            buttonPause.onClick.RemoveListener(OnPauseClicked);

        if (input != null) 
            input.OnPlayerKeycodeEnterPerformed -= OnPlayerKeycodeEnterPerformed;

        InteractionEvents.OnTabPerformed -= SyncHintToggleOn;
        InteractionEvents.OnTabCanceled -= SyncHintToggleOff;
    }

    // ==========================================
    // PAUSE LOGIC
    // ==========================================
    
    private void OnPauseClicked()
    {
        OnPauseRequest?.Invoke();
    }

    // ==========================================
    // WRAP UP LOGIC
    // ==========================================

    private void OnWrapUpClick()
    {
        OnWrapUp?.Invoke();
    }

    public void EnableWrapUp(bool canWrapUp)
    {
        this.canWrapUp = canWrapUp;
    }

    public void ShowButtonWrap(bool isShowing)
    {
        if (activeWrapUpButton == null) return;

        GameObject targetObj = activeWrapUpButton == buttonWrapUp 
            ? activeWrapUpButton.transform.parent.gameObject 
            : activeWrapUpButton.gameObject;

        bool wasShowing = targetObj.activeSelf;
        targetObj.SetActive(isShowing);
        
        if (isShowing && !wasShowing)
        {
            AudioEvents.TriggerPlayCustomSFX(Modules.SoundSystems.AudioKey.SFX_Finish);
        }
    }

    private void OnPlayerKeycodeEnterPerformed()
    {
        if (canWrapUp)
        {
            activeWrapUpButton?.OnClick?.Invoke();
        }
    }

    // ==========================================
    // HINT / CLUE TOGGLE LOGIC
    // ==========================================

    private void OnHintToggleValueChanged(bool isOn)
    {
        if (isOn) InteractionEvents.OnTabPerformed?.Invoke();
        else InteractionEvents.OnTabCanceled?.Invoke();
    }

    private void SyncHintToggleOn()
    {
        if (hintToggle != null && !hintToggle.isOn) hintToggle.SetIsOnWithoutNotify(true); 
    }

    private void SyncHintToggleOff()
    {
        if (hintToggle != null && hintToggle.isOn) hintToggle.SetIsOnWithoutNotify(false);
    }

    public void ShowHintToggle(bool isShowing)
    {
        if (hintToggle != null)
        {
            bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            isMobile = true;
#endif
            hintToggle.gameObject.SetActive(isShowing && isMobile);
        }
    }
}