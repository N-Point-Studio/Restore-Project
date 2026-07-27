using UnityEngine;

public interface IDraggableTool
{
    void StickToSurface(Vector3 position, Quaternion rotation);
    void PlaySfx(bool isPlaying);
    void PlayVfx(bool isPlaying);
    Transform GetTransform();
}

public interface IDraggableBrushTool
{
    Texture2D GetBrush();
    float GetBrushScale();
    float GetBrushStrength();
    Color GetBrushColor();
    Transform GetBrushTransform();
    float GetBrushDepth();
    float GetRaycastLength();
}

public interface IDraggableChiselTool
{
    void PlayGouge();
}