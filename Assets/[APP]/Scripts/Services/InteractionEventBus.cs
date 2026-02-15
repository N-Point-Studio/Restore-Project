using System;
using UnityEngine;

public class InteractionEventBus : IInteractionEvent
{
    public event Action<IInteractable, Vector3, bool> OnObjectDropped;

    public void PublishObjectDropped(IInteractable target, Vector3 worldPos, bool isClick)
    {
        OnObjectDropped?.Invoke(target, worldPos, isClick);
    }
}
