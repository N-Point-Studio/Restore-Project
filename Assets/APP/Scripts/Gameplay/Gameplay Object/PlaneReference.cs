using UnityEngine;

public class PlaneReference : MonoBehaviour
{
    public void SetReferencePosition(Vector3 newValue)
    {
        transform.position = newValue;
    }
}
