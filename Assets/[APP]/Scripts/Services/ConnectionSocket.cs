using UnityEngine;

[System.Serializable]
public class ConnectionSocket
{
    public string targetPieceId;
    [SerializeField] public Transform transform;
    public bool isOccupied;
}