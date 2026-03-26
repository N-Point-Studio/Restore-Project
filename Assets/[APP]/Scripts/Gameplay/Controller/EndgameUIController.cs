using System;
using UnityEngine;
using VContainer;

public class EndgameUIController : BaseMenuController
{
    [SerializeField] private ButtonInputInstructionUI buttonFinish;

    private InputSystemService input;

    public event Action OnFinishedGame;

    [Inject]
    public void Construct(InputSystemService input)
    {
        this.input = input;
        this.input.OnUIKeycodeEnterPerformed += OnUIKeycodeEnterPerformed;
    }

    protected override void Awake()
    {
        base.Awake();
        buttonFinish.OnClick += OnFinishClick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonFinish.OnClick -= OnFinishClick;        
        input.OnUIKeycodeEnterPerformed -= OnUIKeycodeEnterPerformed;
    }

    private void OnFinishClick()
    {
        OnFinishedGame?.Invoke();
    }

    private void OnUIKeycodeEnterPerformed()
    {
        buttonFinish.OnClick?.Invoke();
    }
}
