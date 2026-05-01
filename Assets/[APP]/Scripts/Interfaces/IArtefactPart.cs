using System.Collections.Generic;
using UnityEngine;

public interface IArtefactPart : IAssemble
{
    string PieceId { get; }
    ConnectionSocket GetAvailableSocketFor(string id);
    void ReleaseSocketWith(string otherId);
    List<ConnectionSocket> GetSockets();
    void CorrectRotation(Quaternion rotation);
    ArtefactPieceState CurrentState { get; }
    bool IsSlotEmpty();
}