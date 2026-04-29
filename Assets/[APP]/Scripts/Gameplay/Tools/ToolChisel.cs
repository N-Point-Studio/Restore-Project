using UnityEngine;
using DG.Tweening;

public class ToolChisel : Tool, IClickTool
{
    // [SerializeField] private float offsetForward = 0.005f;
    public void PlayAnimation()
    {
        // transform.DOKill(true);

        // float originalZ = transform.localPosition.z;

        // Sequence seq = DOTween.Sequence();

        // seq.Append(transform.DOLocalMoveZ(originalZ + offsetForward, 0.08f).SetEase(Ease.OutQuad));
        // seq.Append(transform.DOLocalMoveZ(originalZ, 0.12f).SetEase(Ease.InQuad));
    }

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void InteractDetected() { }
    protected override void InteractEnded() { }
    protected override void PressEnded() { }
    protected override void PressStarted() { }
    protected override void ToolVFX(bool isPlaying) { }
}