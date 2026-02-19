using UnityEngine;

[System.Serializable]
public class ConnectionSocket
{
    public string targetPieceId;
    // public string requiredPieceId;
    [SerializeField] public Transform transform;
    public bool isOccupied;
}