using System.Collections.Generic;
using UnityEngine;

public interface IArtefactPart : IAssemble
{
    string PieceId { get; }
    ConnectionSocket GetAvailableSocketFor(string id);
    void ReleaseSocketWith(string otherId);
    List<ConnectionSocket> GetSockets();
    Renderer GetRenderer();
    void CorrectRotation(Quaternion rotation);
}