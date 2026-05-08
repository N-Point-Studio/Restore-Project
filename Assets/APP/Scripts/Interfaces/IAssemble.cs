using System.Collections.Generic;
using UnityEngine;

public interface IAssemble
{
    Transform GetTransform();
    void OnAssembled(Transform targetTransform);
    void OnDetached();
}