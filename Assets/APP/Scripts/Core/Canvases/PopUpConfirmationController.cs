using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class PopUpConfirmationController : BaseMenuController
{
    [Header("UI Texts")]
    [SerializeField] private TMP_Text textTitle;
    [SerializeField] private TMP_Text textSubtitle;

    [Header("Localization Data")]
    [SerializeField] private LocalizedString localizedTitle;
    [SerializeField] private LocalizedString localizedSubtitle;

    [Header("Buttons")]
    [SerializeField] private ButtonItemUI buttonConfirm;
    [SerializeField] private ButtonItemUI buttonCancel;

    [Header("Button Overrides (Optional)")]
    [Tooltip("Leave empty to use the default button localization inside the ButtonItemUI prefab")]
    [SerializeField] private LocalizedString overrideConfirmLabel;
    [Tooltip("Leave empty to use the default button localization inside the ButtonItemUI prefab")]
    [SerializeField] private LocalizedString overrideCancelLabel;

    public event Action OnConfirm;
    public event Action OnCancel;

    protected override void Awake()
    {
        base.Awake();
        buttonConfirm.OnClick += OnButtonConfirmClick;
        buttonCancel.OnClick += OnButtonCancelClick;
    }

    protected virtual void OnEnable()
    {
        if (localizedTitle != null) localizedTitle.StringChanged += UpdateTitleText;
        if (localizedSubtitle != null) localizedSubtitle.StringChanged += UpdateSubtitleText;

        if (overrideConfirmLabel != null && !overrideConfirmLabel.IsEmpty)
        {
            buttonConfirm.SetLocalizedLabel(overrideConfirmLabel);
        }

        if (overrideCancelLabel != null && !overrideCancelLabel.IsEmpty)
        {
            buttonCancel.SetLocalizedLabel(overrideCancelLabel);
        }
    }

    protected virtual void OnDisable()
    {
        if (localizedTitle != null) localizedTitle.StringChanged -= UpdateTitleText;
        if (localizedSubtitle != null) localizedSubtitle.StringChanged -= UpdateSubtitleText;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonConfirm.OnClick -= OnButtonConfirmClick;
        buttonCancel.OnClick -= OnButtonCancelClick;
    }

    private void UpdateTitleText(string value)
    {
        if (textTitle != null) textTitle.text = value;
    }

    private void UpdateSubtitleText(string value)
    {
        if (textSubtitle != null) textSubtitle.text = value;
    }

    // 6. Updated Setters to take LocalizedString instead of standard strings
    public void SetTitle(LocalizedString newTitle)
    {
        if (localizedTitle != null) localizedTitle.StringChanged -= UpdateTitleText;
        
        localizedTitle = newTitle;
        
        if (localizedTitle != null)
        {
            localizedTitle.StringChanged += UpdateTitleText;
            localizedTitle.RefreshString();
        }
    }

    public void SetSubtitle(LocalizedString newSubtitle)
    {
        if (localizedSubtitle != null) localizedSubtitle.StringChanged -= UpdateSubtitleText;
        
        localizedSubtitle = newSubtitle;
        
        if (localizedSubtitle != null)
        {
            localizedSubtitle.StringChanged += UpdateSubtitleText;
            localizedSubtitle.RefreshString();
        }
    }

    private void OnButtonConfirmClick()
    {
        OnConfirm?.Invoke();
    }

    private void OnButtonCancelClick()
    {
        OnCancel?.Invoke();
    }
}