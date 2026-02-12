using UnityEngine;

public interface IHoldable
{
    void OnHoldStart();
    void OnHoldPerformed();
    void OnHoldEnd();
}
