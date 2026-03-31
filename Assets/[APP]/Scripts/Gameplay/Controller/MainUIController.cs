using System;
using UnityEngine;
using VContainer;

public class MainUIController : BaseMenuController
{
    [SerializeField] private ButtonInputInstructionUI buttonWrapUp;

    private InputSystemService input;
    private bool canWrapUp;
    public event Action OnWrapUp;

    [Inject]
    public void Construct(InputSystemService input)
    {
        this.input = input;
        this.input.OnPlayerKeycodeEnterPerformed += OnPlayerKeycodeEnterPerformed;
    }

    protected override void Awake()
    {
        base.Awake();
        buttonWrapUp.OnClick += OnWrapUpClick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonWrapUp.OnClick -= OnWrapUpClick;
        input.OnPlayerKeycodeEnterPerformed -= OnPlayerKeycodeEnterPerformed;
    }

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
        buttonWrapUp.transform.parent.gameObject.SetActive(isShowing);
        if (isShowing)
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
}
