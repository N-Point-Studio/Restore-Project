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
    SurfaceDetectionType ToolType { get; }
}