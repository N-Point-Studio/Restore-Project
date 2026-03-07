using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtefactDetailController : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroupRoot;
    [SerializeField] private Sprite defaultBgSprite;
    [SerializeField] private Image imageBg;
    [SerializeField] private Button buttonClean;
    [SerializeField] private ButtonItemUI buttonBack;

    [Header("Restore Panel")]
    [SerializeField] private GameObject panelRestore;
    [SerializeField] private Image imageIconRestore;
    [SerializeField] private RectTransform imageIconRectRestore;
    [SerializeField] private TMP_Text textTitle;
    [SerializeField] private TMP_Text textDescription;
    [SerializeField] private TMP_Text textFooter;

    [Header("Unrestore Panel")]
    [SerializeField] private GameObject panelUnrestore;
    [SerializeField] private Image imageIconUnrestore;
    [SerializeField] private RectTransform imageIconRectUnrestore;

    [Header("Settings")]
    [SerializeField] private float fadeDuration;

    private ArtefactData currentArtefactData;

    public RectTransform TargetIconRectRestore => imageIconRectRestore;
    public RectTransform TargetIconRectUnrestore => imageIconRectUnrestore;

    private void Awake()
    {
        buttonBack.OnClick += OnClickBack;
        buttonClean.onClick.AddListener(OnClickClean);
        root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (buttonBack) buttonBack.OnClick -= OnClickBack;
        if (buttonClean) buttonClean.onClick.RemoveListener(OnClickClean);
        transform.DOKill();
    }

    public void OpenDetail(ArtefactData artefactData, Sprite bgSprite, bool isCompleted)
    {
        currentArtefactData = artefactData;

        imageBg.sprite = bgSprite != null ? bgSprite : defaultBgSprite;

        if (currentArtefactData == null) return;
        
        panelRestore.SetActive(isCompleted);
        panelUnrestore.SetActive(!isCompleted);

        if (isCompleted)
        {
            imageIconRestore.sprite = currentArtefactData.CompletedIcon;
            textTitle.text = currentArtefactData.BaseData.ItemName;
            textDescription.text = currentArtefactData.BaseData.ItemDescription;
            textFooter.text = $"Code: {currentArtefactData.BaseData.Id}\nConserved by Nova";
        }
        else
        {
            imageIconUnrestore.sprite = currentArtefactData.BaseData.ItemIcon;
        }

        HideContentInstant();

        root.SetActive(true);
    }

    public void CloseDetail()
    {
        if (canvasGroupRoot != null)
        {
            canvasGroupRoot.DOFade(0f, 0.3f).OnComplete(() =>
            {
                root.SetActive(false);
                canvasGroupRoot.alpha = 1f;
            });
        }
        else
        {
            root.SetActive(false);
        }
    }

    public void HideContentInstant()
    {
        if (canvasGroupRoot)
        {
            canvasGroupRoot.alpha = 0f;
            canvasGroupRoot.blocksRaycasts = false;
        }

        if (imageBg) SetAlpha(imageBg, 0f);
        if (imageIconRestore) SetAlpha(imageIconRestore, 0f);
    }

    public void ShowContentFadeIn()
    {
        Sequence seq = DOTween.Sequence();

        if (canvasGroupRoot) canvasGroupRoot.blocksRaycasts = true;

        if (imageBg) seq.Join(imageBg.DOFade(1f, fadeDuration));
        if (imageIconRestore) seq.Join(imageIconRestore.DOFade(1f, fadeDuration));
        if (canvasGroupRoot) seq.Join(canvasGroupRoot.DOFade(1f, fadeDuration));
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private void OnClickBack()
    {
        MainMenuEvents.TriggerCloseArtefactDetail();
    }

    private void OnClickClean()
    {
        if (currentArtefactData == null) return;
        MainMenuEvents.TriggerArtefactPlay(currentArtefactData);
    }
}