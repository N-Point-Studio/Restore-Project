using UnityEngine;

public interface IRotateable
{
    void OnRotateStarted();
    void OnRotatePerformed(Vector2 delta);
    void OnRotateEnd();
}