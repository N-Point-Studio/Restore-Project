using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public SlotSocket slotSocket;
    public List<Slot> requiredSocket;

    public bool IsAllRequiredCompleted()
    {
        return false;
    }
}
