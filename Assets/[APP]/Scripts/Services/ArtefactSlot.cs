using System.Collections.Generic;
using UnityEngine;

public class ArtefactSlot : MonoBehaviour
{
    public string targetPieceId;
    public bool isOccupied;
    public Transform position;
    public List<ArtefactSlot> requiredSocket;

    public bool IsRequiredSocketOccupied()
    {
        foreach (var socket in requiredSocket)
        {
            if (!socket.isOccupied)
                return false;
        }
        return true;
    }

    public bool CanAccept(ArtefactPieceStateMachine piece)
    {
        if (isOccupied) return false;
        if (piece.pieceId != targetPieceId) return false;
        if (!IsRequiredSocketOccupied()) return false;
        return true;
    }
}
