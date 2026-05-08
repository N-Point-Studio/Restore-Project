using System;
using TMPro;
using UnityEngine;
using VContainer;

public class EndgameUIController : BaseMenuController
{
    [SerializeField] private TMP_Text textArtefactName;
    [SerializeField] private ButtonInputInstructionUI buttonFinish;

    private InputSystemService input;    
    private PlayerProgressionData playerProgressionData;
    private ActiveArtefactData activeArtefactData;

    public event Action OnFinishedGame;

    [Inject]
    public void Construct(
        InputSystemService input, 
        PlayerProgressionData playerProgressionData, 
        ActiveArtefactData activeArtefactData)
    {
        this.input = input;
        this.playerProgressionData = playerProgressionData;
        this.activeArtefactData = activeArtefactData;
        
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
                    SetArtefactName(data.BaseData.ItemName);
                }
            }
        }
    }

    public void SetArtefactName(string name)
    {
        if (textArtefactName != null)
        {
            textArtefactName.text = name;
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