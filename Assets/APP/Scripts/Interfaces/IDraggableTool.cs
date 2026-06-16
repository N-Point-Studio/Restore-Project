using UnityEngine;

public interface IDraggableTool
{
    void StickToSurface(Vector3 position, Quaternion rotation);
    void PlaySfx(bool isPlaying);
    void PlayVfx(bool isPlaying);
}