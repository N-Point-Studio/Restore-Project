using UnityEngine;

public interface IInteractObject
{
    void OnInteractDetected();
    void OnInteractEnded();
    void SetColliderEnable(bool isActive);
}
