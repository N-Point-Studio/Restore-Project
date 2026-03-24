using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInputInstructionUI : InputInstructionUI
{
    [SerializeField] protected Button button;
    public Button Button => button;

    public Action OnClick;

    protected override void Awake()
    {
        base.Awake();
        button.onClick.AddListener(HandleOnButtonClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        button.onClick.RemoveListener(HandleOnButtonClicked);
    }

    private void HandleOnButtonClicked()
    {
        OnClick?.Invoke();
    }
}
