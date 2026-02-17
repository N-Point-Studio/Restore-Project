using UnityEngine;

public interface IRotate : IInteract
{
    void OnRotatePerformed(Vector2 delta);
}