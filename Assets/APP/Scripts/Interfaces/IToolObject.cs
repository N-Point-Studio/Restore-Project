using System;
using UnityEngine;

public interface IToolObject
{
    ToolType ToolId { get; }
    void FollowMouse(Vector3 worldPos);
    void Use();
    void Return();
    void StickToSurface(Vector3 position, Quaternion rotation);
    bool IsStickingToSurface { get; }
    IInteractObject GetOrigin();
    void PlaySfx(bool isPlaying);
    void PlayVfx(bool isPlaying);
}

public interface IBrushTool
{
    Texture2D GetBrush { get; }
    float BrushScale { get; }
    float BrushStrength { get; }
    Color BrushColor { get; }
    Transform BrushTransform { get; }
    float BrushDepth { get; }
}

public interface IChiselTool
{
    void PlayGouge();
}