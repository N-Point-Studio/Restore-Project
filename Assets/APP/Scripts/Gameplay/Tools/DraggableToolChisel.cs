using UnityEngine;

public class DraggableToolChisel : DraggableTool, IDraggableChiselTool
{
    private const float CrossFadeDuration = 0.1f;
    private readonly int GougeHash = Animator.StringToHash("Chisel_Gouge");

    protected override void Awake()
    {
        base.Awake();
    }

    public void PlayGouge()
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.CrossFadeInFixedTime(GougeHash, CrossFadeDuration);
        }
    }

    protected override void ToolVFX(bool isPlaying) { }
}
