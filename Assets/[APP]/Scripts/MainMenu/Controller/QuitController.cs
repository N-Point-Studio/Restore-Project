using UnityEngine;
using VContainer;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitController : BaseMenuController
{
    [SerializeField] private ButtonInputInstructionUI buttonCancel;
    [SerializeField] private ButtonInputInstructionUI buttonConfirm;

    private Tween quitTween;

    protected override void Awake()
    {
        base.Awake();
        buttonCancel.OnClick += OnButtonCancelClick;
        buttonConfirm.OnClick += OnButtonConfirmClick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        buttonCancel.OnClick += OnButtonCancelClick;
        buttonConfirm.OnClick += OnButtonConfirmClick;

        if (quitTween != null && quitTween.IsActive())
        {
            quitTween.Kill();
        }
    }

    private void OnButtonCancelClick()
    {
        SetActive(false);
    }

    private void OnButtonConfirmClick()
    {
        // TODO: Save All Before Quitting?
        if (quitTween != null && quitTween.IsActive()) return;

        quitTween = DOVirtual.DelayedCall(0.05f, () =>
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }).SetUpdate(true);
    }
}
