using UnityEngine;

public interface ICleanable
{
    bool IsCleanable();
}

public interface ICleanSurfaceObject : ICleanable
{
    void TryClean(Vector2 uv, Texture2D brush);
}

public interface IClean
{
    bool IsCleanable();
    public void ForceClean();
}

public interface ICleanSurface : IClean
{
    void CleanSurface(Vector3 hitPoint, Texture2D brush, Vector3 hitNormal, Vector3 direction, float scale, float strength);
    float GetCleaningProgress();
    void ShowClue(bool isShowing);
    bool IsDustRemoved();
}