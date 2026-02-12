using UnityEngine;

public class ClickTouch : MonoBehaviour, IClickable
{
    public void OnClickedEnd()
    {
        Debug.Log("Click ended");
    }

    public void OnClickedPerformed()
    {
        Debug.Log("Click performed");
    }
}
