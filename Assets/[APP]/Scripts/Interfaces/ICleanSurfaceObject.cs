using UnityEngine;

public interface ICleanable
{
    bool IsCleanable();
}

public interface ICleanSurfaceObject : ICleanable
{
    void TryClean(Vector2 uv, Texture2D brush);
}