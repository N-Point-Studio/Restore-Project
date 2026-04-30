using UnityEngine;
using DG.Tweening;

public class ToolChisel : Tool, IChiselTool
{
    private const float CrossFadeDuration = 0.1f;
    private readonly int GougeHash = Animator.StringToHash("Chisel_Gouge");

    public void PlayGouge()
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.CrossFadeInFixedTime(GougeHash, CrossFadeDuration);
        }
    }

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void InteractDetected() { }
    protected override void InteractEnded() { }
    protected override void Moving() { }
    protected override void PressEnded() { }
    protected override void PressStarted() { }
    protected override void StickSurface(float x, float y) { }
    protected override void ToolVFX(bool isPlaying) { }
}