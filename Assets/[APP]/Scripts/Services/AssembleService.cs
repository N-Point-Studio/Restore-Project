using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class AssemblyService
{
    private readonly FragmentService registry;
    private readonly InteractionService interaction;

    [Inject]
    public AssemblyService(FragmentService registry, InteractionService interaction)
    {
        this.registry = registry;
        this.interaction = interaction;
    }

    public bool TryAssemble(ArtefactPieceStateMachine root, ArtefactPieceStateMachine incoming)
    {
        var parentSocket = root.GetAvailableSocketFor(incoming.pieceId);
        if (parentSocket != null)
        {
            PerformAssembly(root, incoming, parentSocket);
            return true;
        }
        else
        {
            var potentialParents = root.GetComponentsInChildren<ArtefactPieceStateMachine>();
            foreach (var parentPart in potentialParents)
            {
                ConnectionSocket socket = parentPart.GetAvailableSocketFor(incoming.pieceId);
                if (socket != null)
                {
                    PerformAssembly(parentPart, incoming, socket);
                    return true;
                }
            }
            return false;
        }
    }

    private void PerformAssembly(ArtefactPieceStateMachine parent, ArtefactPieceStateMachine piece, ConnectionSocket socket)
    {
        piece.transform.SetParent(parent.transform);
        piece.transform.localPosition = socket.transform.localPosition;
        piece.transform.localRotation = socket.transform.localRotation;

        socket.isOccupied = true;

        piece.OnAssembled(parent.transform);
        piece.GetInspectable()?.EnterInspect();

        Debug.Log($"Assembled: {piece.pieceId} attached to {parent.pieceId}");
    }

    public void Detach(ArtefactPieceStateMachine piece)
    {
        if (piece.transform.parent == null) return;

        var parentPiece = piece.transform.parent.GetComponent<ArtefactPieceStateMachine>();
        if (parentPiece != null)
        {
            var socket = parentPiece.sockets.Find(s => s.targetPieceId == piece.pieceId);
            if (socket != null) socket.isOccupied = false;
        }

        piece.transform.SetParent(null);
        piece.OnDetached();

        Debug.Log($"Detached: {piece.pieceId}");
    }
}