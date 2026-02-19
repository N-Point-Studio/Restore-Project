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

    public bool TryAssemble(ArtefactPieceStateMachine root, ArtefactPieceStateMachine incoming)
    {
        var parentSocket = root.GetAvailableSocketFor(incoming.pieceId);
        var childSocket = incoming.GetAvailableSocketFor(root.pieceId);

        if (parentSocket != null && childSocket != null)
        {
            PerformAssembly(root, incoming, parentSocket, childSocket);
            return true;
        }

        var potentialParents = root.GetComponentsInChildren<ArtefactPieceStateMachine>();
        foreach (var parentPart in potentialParents)
        {
            if (parentPart == root) continue;
            var pSocket = parentPart.GetAvailableSocketFor(incoming.pieceId);
            var cSocket = incoming.GetAvailableSocketFor(parentPart.pieceId);
            if (pSocket != null && cSocket != null)
            {
                PerformAssembly(parentPart, incoming, pSocket, cSocket);
                return true;
            }
        }

        return false;
    }

    private void PerformAssembly(ArtefactPieceStateMachine parent, ArtefactPieceStateMachine piece, ConnectionSocket socket, ConnectionSocket parentSocket)
    {
        piece.transform.SetParent(parent.transform);
        piece.transform.localPosition = socket.transform.localPosition;
        piece.transform.localRotation = socket.transform.localRotation;

        socket.isOccupied = true;
        parentSocket.isOccupied = true;

        piece.OnAssembled(parent);
        piece.GetInspectable()?.EnterInspect();
        Debug.Log($"Assembled: {piece.pieceId} attached to {parent.pieceId}, progress: {registry.GetAssemblyProgress()}");
    }

    public void Detach(ArtefactPieceStateMachine piece)
    {
        var parentTransform = piece.transform.parent;
        if (parentTransform == null) return;

        var parentPiece = parentTransform.GetComponent<ArtefactPieceStateMachine>();
        if (parentPiece == null) return;

        if (!(parentPiece.GetCurrentState() is ArtefactPieceInspectState))
        {
            Debug.Log("Detach blocked: Parent not in Inspect state");
            return;
        }

        var parentSocket = parentPiece.sockets.Find(s => s.targetPieceId == piece.pieceId);
        if (parentSocket != null) parentSocket.isOccupied = false;

        var childSocket = piece.sockets.Find(s => s.targetPieceId == parentPiece.pieceId);
        if (childSocket != null) childSocket.isOccupied = false;

        piece.transform.SetParent(null);
        piece.OnDetached();
        Debug.Log($"Detached: {piece.pieceId} attached to {parentPiece.pieceId}, progress: {registry.GetAssemblyProgress()}");
    }
}