using System.Collections.Generic;
using UnityEngine;
using VContainer;
public class AssemblyService
{
    private readonly FragmentService registry;

    [Inject]
    public AssemblyService(FragmentService registry) => this.registry = registry;
    public ArtefactPieceStateMachine GetTopParent(ArtefactPieceStateMachine current)
    {
        var walker = current;
        while (walker.transform.parent != null &&
               walker.transform.parent.GetComponent<ArtefactPieceStateMachine>() != null)
        {
            walker = walker.transform.parent.GetComponent<ArtefactPieceStateMachine>();
        }
        return walker;
    }

    public bool TryAssemble(ArtefactPieceStateMachine root, ArtefactPieceStateMachine incoming)
    {
        var rootParts = root.GetComponentsInChildren<ArtefactPieceStateMachine>();
        var incomingParts = incoming.GetComponentsInChildren<ArtefactPieceStateMachine>();

        foreach (var pPart in rootParts)
        {
            foreach (var iPart in incomingParts)
            {
                var pSocket = pPart.GetAvailableSocketFor(iPart.pieceId);
                var iSocket = iPart.GetAvailableSocketFor(pPart.pieceId);

                if (pSocket != null && iSocket != null)
                {
                    // Aturan Re-parenting: Jika iPart punya bapak lain, lepaskan dulu
                    if (iPart.parent != null && iPart.parent != pPart)
                    {
                        ForceDetachFromCurrentParent(iPart);
                    }

                    PerformAssembly(pPart, iPart, pSocket, iSocket);

                    // Rekursif: Cek apakah teman-teman iPart lainnya bisa ikut nempel ke struktur baru
                    RecursiveCheckReassembly(iPart, incomingParts);
                    return true;
                }
            }
        }
        return false;
    }

    public ArtefactPieceStateMachine RebuildChildrenHierarchy(ArtefactPieceStateMachine parent)
    {
        var children = parent.GetComponentsInChildren<ArtefactPieceStateMachine>();
        if (children.Length <= 1) return null;

        var childList = new List<ArtefactPieceStateMachine>();
        foreach (var c in children) if (c != parent) childList.Add(c);

        // Putus hubungan dari bapak lama agar bebas mencari pasangan baru
        foreach (var child in childList) ForceDetachFromCurrentParent(child);

        bool anyConnectionMade = false;
        foreach (var c1 in childList)
        {
            foreach (var c2 in childList)
            {
                if (c1 == c2) continue;
                if (TryAssemble(c1, c2)) anyConnectionMade = true;
            }
        }

        return anyConnectionMade ? GetTopParent(childList[0]) : null;
    }

    public void Detach(ArtefactPieceStateMachine piece)
    {
        if (piece.parent == null) return;
        ForceDetachFromCurrentParent(piece);
        piece.OnDetached();
        LogProgress("Detached", piece.pieceId);
    }

    private void ForceDetachFromCurrentParent(ArtefactPieceStateMachine piece)
    {
        var oldParent = piece.parent;
        if (oldParent == null) return;

        oldParent.sockets.Find(s => s.targetPieceId == piece.pieceId && s.isOccupied).isOccupied = false;
        piece.sockets.Find(s => s.targetPieceId == oldParent.pieceId && s.isOccupied).isOccupied = false;

        piece.transform.SetParent(null);
        piece.parent = null;
    }

    private void PerformAssembly(ArtefactPieceStateMachine parent, ArtefactPieceStateMachine piece, ConnectionSocket pSocket, ConnectionSocket iSocket)
    {
        piece.transform.SetParent(parent.transform);
        piece.transform.localPosition = pSocket.transform.localPosition;
        piece.transform.localRotation = pSocket.transform.localRotation;

        pSocket.isOccupied = true;
        iSocket.isOccupied = true;
        piece.parent = parent;

        piece.OnAssembled(parent);
        LogProgress("Assembled", piece.pieceId);
    }

    private void RecursiveCheckReassembly(ArtefactPieceStateMachine newParent, ArtefactPieceStateMachine[] potentialChildren)
    {
        foreach (var child in potentialChildren)
        {
            if (child == newParent || child.parent != null) continue;

            var pSocket = newParent.GetAvailableSocketFor(child.pieceId);
            var iSocket = child.GetAvailableSocketFor(newParent.pieceId);

            if (pSocket != null && iSocket != null)
            {
                PerformAssembly(newParent, child, pSocket, iSocket);
                RecursiveCheckReassembly(child, potentialChildren);
            }
        }
    }

    private void LogProgress(string action, string id)
    {
        float progress = registry.GetAssemblyProgress();
        Debug.Log($"<color=cyan>[{action}]</color> {id}. Progress: {progress * 100:F0}%");
    }
}