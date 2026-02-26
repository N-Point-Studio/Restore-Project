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

    public IAssembled GetAssembledRoot(IAssembled piece)
    {
        var temporary = piece;
        while (temporary.GetAssembleParrent() != null)
        {
            temporary = temporary.GetAssembleParrent();
        }

        return temporary;
    }

    public bool TryAssemble(IAssembled root, IAssembled incoming)
    {
        var rootParts = root.GetTransform().GetComponentsInChildren<IAssembled>();
        var incomingParts = incoming.GetTransform().GetComponentsInChildren<IAssembled>();

        foreach (var rootPart in rootParts)
        {
            foreach (var incomingPart in incomingParts)
            {
                var rootSocket = rootPart.GetAvailableSocketFor(incomingPart.PieceId);
                var incomingSocket = incomingPart.GetAvailableSocketFor(rootPart.PieceId);
                if (rootSocket != null && incomingSocket != null)
                {
                    if (incomingPart.GetAssembleParrent() != null && incomingPart.GetAssembleParrent() != rootPart)
                    {
                        ForceDetachFromCurrentParent(incomingPart);
                    }
                    PerformAssembly(rootPart, incomingPart, rootSocket, incomingSocket);
                    RecursiveCheckReassembly(incomingPart, incomingParts);
                    LogProgress("Assembled", incomingPart.PieceId);
                    return true;
                }
            }
        }
        return false;
    }

    public void Detach(IAssembled piece)
    {
        IAssembled parent = piece.GetAssembleParrent();
        if (parent == null) return;
        parent.ReleaseSocketWith(piece.PieceId);
        piece.ReleaseSocketWith(parent.PieceId);
        piece.GetTransform().SetParent(null);
        piece.OnDetached();
        LogProgress("Detached", piece.PieceId);
    }

    private void ForceDetachFromCurrentParent(IAssembled piece)
    {
        IAssembled oldParent = piece.GetAssembleParrent();
        if (oldParent == null) return;

        oldParent.ReleaseSocketWith(piece.PieceId);
        piece.ReleaseSocketWith(oldParent.PieceId);

        Debug.Log($"[Detach] {piece.PieceId} dilepaskan dari {oldParent.PieceId}");
    }

    private void PerformAssembly(IAssembled parent, IAssembled incoming, ConnectionSocket parentSocket, ConnectionSocket incomingSocket)
    {
        incoming.GetTransform().SetParent(parent.GetTransform());
        // incoming.GetTransform().SetLocalPositionAndRotation(parentSocket.transform.localPosition, parentSocket.transform.localRotation);
        parentSocket.isOccupied = true;
        incomingSocket.isOccupied = true;

        incoming.OnAssembled(parent, parentSocket.transform);
    }

    private void RecursiveCheckReassembly(IAssembled newParent, IAssembled[] potentialChildren)
    {
        foreach (var child in potentialChildren)
        {
            if (child == newParent || child.GetAssembleParrent() != null) continue;
            var parentSocket = newParent.GetAvailableSocketFor(child.PieceId);
            var incomingSocket = child.GetAvailableSocketFor(newParent.PieceId);
            if (parentSocket != null && incomingSocket != null)
            {
                PerformAssembly(newParent, child, parentSocket, incomingSocket);
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