using UnityEngine;

public interface ICleanSurfaceObject
{
    void TryClean(Vector2 uv, Texture2D brush);
}