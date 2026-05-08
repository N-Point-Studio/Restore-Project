using System;
using TMPro;
using UnityEngine;

public class PopUpConfirmationController : BaseMenuController
{
    [SerializeField] private TMP_Text textTitle;
    [SerializeField] private TMP_Text textSubtitle;

    [SerializeField] private string titleLabel;
    [SerializeField] private string subtitleLabel;

    [SerializeField] private ButtonItemUI buttonConfirm;
    [SerializeField] private ButtonItemUI buttonCancel;

    public event Action OnConfirm;
    public event Action OnCancel;

    protected override void Awake()
    {
        base.Awake();
        buttonConfirm.OnClick += OnButtonConfirmClick;
        buttonCancel.OnClick += OnButtonCancelClick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonConfirm.OnClick -= OnButtonConfirmClick;
        buttonCancel.OnClick -= OnButtonCancelClick;
    }

    public void SetTitle(string title)
    {
        textTitle.text = title;
    }

    public void SetSubtitle(string subtitle)
    {
        textSubtitle.text = subtitle;
    }

    public void SetConfirmButtonText(string label)
    {
        buttonConfirm.SetLabel(label);
    }

    public void SetCancelButtonText(string label)
    {
        buttonCancel.SetLabel(label);
    }

    private void OnButtonConfirmClick()
    {
        OnConfirm?.Invoke();
    }

    private void OnButtonCancelClick()
    {
        OnCancel?.Invoke();
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(titleLabel) && textTitle != null)
        {
            textTitle.text = titleLabel;
        }

        if (!string.IsNullOrEmpty(subtitleLabel) && textSubtitle != null)
        {
            textSubtitle.text = subtitleLabel;
        }
    }
#endif


}
