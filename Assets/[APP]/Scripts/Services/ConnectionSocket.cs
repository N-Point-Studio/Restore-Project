using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConnectionSocket
{
    public string targetPieceId;
    // public string requiredPieceId;
    [SerializeField] public Transform transform;
    public bool isOccupied;
}

[System.Serializable]
public class SlotSocket
{
    public string targetPieceId;
    public bool isOccupied;
    [SerializeField] public Transform transform;
}