using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using VContainer;
using UnityEngine.EventSystems;

public class EndgameUIController : BaseMenuController, IDragHandler
{
    [SerializeField] private TMP_Text textArtefactName;
    [SerializeField] private ButtonInputInstructionUI buttonFinish;

    private InputSystemService input;
    private PlayerProgressionData playerProgressionData;
    private ActiveArtefactData activeArtefactData;
    private LocalizedString currentLocalizedName;
    private Inspection inspection;

    public event Action OnFinishedGame;

    [Inject]
    public void Construct(
        InputSystemService input,
        PlayerProgressionData playerProgressionData,
        ActiveArtefactData activeArtefactData,
        Inspection inspection)
    {
        this.input = input;
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        this.inspection = inspection;

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

        if (currentLocalizedName != null) currentLocalizedName.StringChanged -= UpdateArtefactNameText;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inspection != null)
        {
            inspection.RotateHorizontally(eventData.delta.x);
        }
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);

        if (isActive)
        {
            RefreshArtefactName();
        }
    }

    private void RefreshArtefactName()
    {
        if (playerProgressionData != null && activeArtefactData != null)
        {
            string targetId = playerProgressionData.CurrentActiveArtefactId;

            if (!string.IsNullOrEmpty(targetId))
            {
                ArtefactData data = activeArtefactData.GetArtefactDatabase().GetItem(targetId);
                if (data != null)
                {
                    SetArtefactName(data.LocalizedItemName);
                }
            }
        }
    }

    public void SetArtefactName(LocalizedString localizedName)
    {
        if (currentLocalizedName != null) currentLocalizedName.StringChanged -= UpdateArtefactNameText;

        currentLocalizedName = localizedName;

        if (currentLocalizedName != null)
        {
            currentLocalizedName.StringChanged += UpdateArtefactNameText;
            currentLocalizedName.RefreshString();
        }
    }

    private void UpdateArtefactNameText(string value)
    {
        if (textArtefactName != null)
        {
            textArtefactName.text = value;
        }
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