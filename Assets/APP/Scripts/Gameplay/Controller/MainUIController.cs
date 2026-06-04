using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using UnityEngine.Localization;

public class MainUIController : BaseMenuController
{
    [SerializeField] private ButtonInputInstructionUI buttonWrapUp;

    [Header("Hint / Clue System")]
    [SerializeField] private Toggle hintToggle;

    [Header("Pause System")]
    [SerializeField] private Button buttonPause;

    private InputSystemService input;
    private bool canWrapUp;
    
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
        
        if (buttonWrapUp != null) 
            buttonWrapUp.OnClick += OnWrapUpClick;

        if (hintToggle != null) 
        {
            hintToggle.onValueChanged.AddListener(OnHintToggleValueChanged);
            hintToggle.gameObject.SetActive(false); 
        }

        if (buttonPause != null)
        {
            buttonPause.onClick.AddListener(OnPauseClicked);
            
            bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            isMobile = true;
#endif
            buttonPause.gameObject.SetActive(isMobile);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (buttonWrapUp != null) 
            buttonWrapUp.OnClick -= OnWrapUpClick;
            
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
        bool wasShowing = buttonWrapUp.transform.parent.gameObject.activeSelf;

        buttonWrapUp.transform.parent.gameObject.SetActive(isShowing);
        
        if (isShowing && !wasShowing)
        {
            AudioEvents.TriggerPlayCustomSFX(Modules.SoundSystems.AudioKey.SFX_Finish);
        }
    }

    private void OnPlayerKeycodeEnterPerformed()
    {
        if (canWrapUp)
        {
            buttonWrapUp.OnClick?.Invoke();
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