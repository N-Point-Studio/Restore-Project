using UnityEngine;

public interface IClick : IInteract
{
    void OnClick();
}

public interface IPressObject
{
    void OnPressStarted();
    void OnPressEnded();
}