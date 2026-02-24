using System.Collections.Generic;
using UnityEngine;
using VContainer;
public class AssemblyService
{
    private readonly FragmentService registry;

    [Inject]
    public AssemblyService(FragmentService registry)
    {
        this.registry = registry;
    }

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
                    if (iPart.parent != null && iPart.parent != pPart)
                    {
                        ForceDetachFromCurrentParent(iPart);
                    }

                    PerformAssembly(pPart, iPart, pSocket, iSocket);
                    RecursiveCheckReassembly(iPart, incomingParts);
                    return true;
                }
            }
        }
        return false;
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

    public ArtefactPieceStateMachine TryAssembleParent(ArtefactPieceStateMachine parent)
    {
        // 1. Ambil semua anak (kecuali parent itu sendiri)
        var children = new List<ArtefactPieceStateMachine>(parent.GetComponentsInChildren<ArtefactPieceStateMachine>());
        children.Remove(parent);
        // Debug.Log($"TryAssembleParent: Found {children.Count} children for {parent.name}");
        // Debug.Log($"TryAssembleParent: children 0 is {children[0].name}");

        // Jika anaknya cuma satu, langsung return anak tersebut
        if (children.Count == 1) return children[0];
        // Jika tidak ada anak, return null
        if (children.Count == 0) return null;

        // 2. Detach semua anak dari parent agar bersih (logic & hierarchy)
        foreach (var child in children)
        {
            ForceDetachFromCurrentParent(child);
        }

        bool anyAssembled = false;

        // 3. Cek hubungan antar anak (Nested Loop)
        // Kita gunakan perulangan yang bisa di-reset karena hierarki berubah saat PerformAssembly
        for (int i = 0; i < children.Count; i++)
        {
            for (int j = 0; j < children.Count; j++)
            {
                if (i == j) continue;

                var childA = children[i];
                var childB = children[j];

                // Cek apakah childB sudah punya parent? Kalau sudah, jangan di-assemble lagi
                if (childB.parent != null) continue;

                var socketA = childA.GetAvailableSocketFor(childB.pieceId);
                var socketB = childB.GetAvailableSocketFor(childA.pieceId);

                if (socketA != null && socketB != null)
                {
                    PerformAssembly(childA, childB, socketA, socketB);
                    anyAssembled = true;

                    // Reset pencarian karena struktur sudah berubah
                    i = 0;
                    j = 0;
                }
            }
        }

        // 4. Final Check: Jika ada yang berhasil digabung, return "anak pertama" 
        // (yang sekarang kemungkinan sudah menjadi root dari sub-tree baru)
        if (anyAssembled)
        {
            return children[0];
        }

        // 5. Jika tidak ada yang saling berhubungan, pastikan semua benar-benar ter-detach
        foreach (var child in children)
        {
            ForceDetachFromCurrentParent(child);
        }

        return null;
    }

    private void LogProgress(string action, string id)
    {
        float progress = registry.GetAssemblyProgress();
        Debug.Log($"<color=cyan>[{action}]</color> {id}. Progress: {progress * 100:F0}%");
    }
}