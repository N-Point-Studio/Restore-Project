using System;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactItemUI : MonoBehaviour
{
    [SerializeField] private Image imageIcon;
    [SerializeField] private RectTransform targetIconRect;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Button button;

    private ArtefactData artefactData;
    public ArtefactData ArtefactData => artefactData;
    public RectTransform TargetIconRect => targetIconRect; 
    private Action<ArtefactItemUI> onClickCallback;

    private void Awake()
    {
        button.onClick.AddListener(HandleOnButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleOnButtonClick);
    }

    public void Initialize(ArtefactData artefactData, bool isLocked, bool isCompleted, Action<ArtefactItemUI> onClick)
    {
        this.artefactData = artefactData;
        this.onClickCallback = onClick;
        
        if (imageIcon)
        {
            if (isCompleted)
            {
                 imageIcon.sprite = artefactData.CompletedIcon;
            }
            else if (!isLocked)
            {
                imageIcon.sprite = artefactData.BaseData.ItemIcon;
            }
            else
            {
                imageIcon.sprite = lockedSprite;
            }
        }

        if (button) button.interactable = !isLocked;
    }

    private void HandleOnButtonClick()
    {
        if (artefactData == null) return;
        onClickCallback?.Invoke(this);
    }
}
