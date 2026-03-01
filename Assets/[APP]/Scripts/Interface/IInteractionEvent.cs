using System;
using UnityEngine;

public interface IInteractionEvent
{
    event Action<IInteractable, Vector3, bool> OnObjectDropped;
    void PublishObjectDropped(IInteractable target, Vector3 worldPos, bool isClick);
}