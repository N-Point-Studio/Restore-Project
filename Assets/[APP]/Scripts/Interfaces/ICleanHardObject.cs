using UnityEngine;

public interface ICleanHardObject : ICleanable
{
    int CurrentHit { get; }
    int MaxHit { get; }
    void Hit();
}

public interface ICleanChunk : IClean
{
    void Hit();
}