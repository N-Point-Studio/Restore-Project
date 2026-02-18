using UnityEngine;

public interface IAssembled
{
    void OnAssembled(Transform parent);
    void OnDetached();
}
