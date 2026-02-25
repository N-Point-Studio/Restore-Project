using UnityEngine;

public interface IInspectable
{
    void EnterInspect(Vector3 targetPosition);
    void ExitInspect();
    Transform GetTransform();
}