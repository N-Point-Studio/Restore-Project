using UnityEngine;

public class ToolChisel : Tool
{
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
