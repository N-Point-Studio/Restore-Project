using UnityEngine;

public interface IInteract
{
    void OnInteractStart();
    void OnInteractEnd();
}


public interface IInteractObject
{
    void OnInteractDetected();
    void OnInteractEnded();
}
