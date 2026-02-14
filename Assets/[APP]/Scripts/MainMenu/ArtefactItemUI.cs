using UnityEngine;
using UnityEngine.UI;

public class ArtefactItemUI : MonoBehaviour
{
    [SerializeField] private Image imageIcon;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Button button;

    private ArtefactData artefactData;
    public ArtefactData ArtefactData => artefactData;

    private void Awake()
    {
        button.onClick.AddListener(HandleOnButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleOnButtonClick);
    }

    public void Initialize(ArtefactData artefactData, bool isLocked)
    {
        this.artefactData = artefactData;
        if (imageIcon) imageIcon.sprite = isLocked ? lockedSprite : artefactData.BaseData.ItemIcon;

        if (button) button.interactable = !isLocked;
    }

    private void HandleOnButtonClick()
    {
        if (artefactData == null) return;

        MainMenuEvents.OnRequestArtefactDetail?.Invoke(artefactData);
    }
}
