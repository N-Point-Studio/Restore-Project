using Modules.SoundSystems;
using UnityEngine;

public interface IToolObject
{
    void FollowMouse(Vector3 worldPos);
    void Use();
    void Return();
    void StickToSurface(Vector3 position, Quaternion rotation);
    void SetStickToSurface(bool isSticking);
    bool IsStickingToSurface { get; }
    IInteractObject GetOrigin();
    void PlaySfx(bool isPlaying);
    void PlayVfx(bool isPlaying);
}

public interface IToolOrigin
{
    void CheckOrigin(bool isCheck);
}

public interface IBrushTool
{
    Texture2D GetBrush { get; }
    float BrushScale { get; }
    float BrushStrength { get; }
    Color BrushColor { get; }
    Transform BrushTransform { get; }
}