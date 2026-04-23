using UnityEngine;

public interface ITool
{
    void FollowMouse(Vector3 worldPos);
    void Use();
    void Return();
    void StickToSurface(Vector3 position, Quaternion rotation);
    void PlaySfx(bool isPlaying);
    void PlayVfx(bool isPlaying);
    void Moving(bool isMoving);
    Texture2D GetBrush();
    // SurfaceDetectionType ToolType { get; }
}

public interface IToolObject
{
    void FollowMouse(Vector3 worldPos);
    void Use();
    void Return();
    void StickToSurface(Vector3 position, Quaternion rotation);
    void SetStickToSurface(bool isSticking);
    bool IsStickingToSurface { get; }
    IInteractObject GetOrigin();
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