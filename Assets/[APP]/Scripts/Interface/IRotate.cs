using UnityEngine;

public interface IRotate : IInteract
{
    bool OnRotatePerformed(Vector2 delta);
}