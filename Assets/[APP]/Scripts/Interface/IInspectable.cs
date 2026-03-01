using UnityEngine;

public interface IInspectable
{
    void EnterInspect(Transform targetPosition);
    void ExitInspect();
    Transform GetTransform();
    bool IsInspected { get; }
}