using UnityEngine;

public class ClickTouch : MonoBehaviour, IHoldable
{
    public void OnHoldEnd()
    {
        Debug.Log("hold end");
    }

    public void OnHoldPerformed()
    {
        Debug.Log("hold");
    }
}
