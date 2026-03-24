using UnityEngine;

public interface ICleanHardObject
{
    int CurrentHit { get; }
    int MaxHit { get; }

    void Hit();
}
