using System;
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

        foreach (ConnectionSocket availableSocket in piece.sockets)
        {
            if (availableSocket.targetPieceId == parent.pieceId)
            {
                piece.transform.localPosition = socket.transform.localPosition;
                piece.transform.localRotation = socket.transform.localRotation;

                socket.isOccupied = true;
                parentSocket.isOccupied = true;

                piece.OnAssembled(parent);
                piece.GetInspectable()?.EnterInspect();
                Debug.Log($"Assembled: {piece.pieceId} attached to {parent.pieceId}, progress: {registry.GetAssemblyProgress()}");
            }
            else
            {
                availableSocket.isOccupied = false;
            }
        }
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

    public bool TryDetachParent(ArtefactPieceStateMachine piece)
    {
        var children = piece.GetComponentsInChildren<ArtefactPieceStateMachine>();

        // Tidak punya child (selain dirinya sendiri)
        if (children.Length <= 1)
            return false;

        Debug.Log($"Detaching parent {piece.pieceId} with {children.Length - 1} children");

        // 1️⃣ Release semua socket milik piece
        foreach (var socket in piece.sockets)
        {
            socket.isOccupied = false;
        }

        piece.transform.SetParent(null);

        // 4️⃣ Trigger detach
        piece.OnDetached();

        foreach (var child in children)
        {
            if (child == piece) continue;

            var childSocket = child.sockets.Find(s => s.targetPieceId == piece.pieceId);
            if (childSocket != null)
                childSocket.isOccupied = false;

            // Lepas child dari piece
            child.transform.SetParent(null);

            // Masuk inspect state
            child.GetInspectable()?.EnterInspect();
        }

        // 3️⃣ Lepas piece dari parentnya (kalau ada)

        // 5️⃣ Kalau ada lebih dari 1 child → coba assemble ulang antar child
        if (children.Length > 2)
        {
            TryReassembleChildren(children);
        }

        return true;
    }

    private void TryReassembleChildren(ArtefactPieceStateMachine[] pieces)
    {
        if (pieces == null || pieces.Length <= 2)
        {
            Debug.Log("Not enough pieces to reassemble");
            return;
        }

        // Skip index 0 kalau itu parent lama
        var candidates = new List<ArtefactPieceStateMachine>();

        foreach (var p in pieces)
        {
            if (p == null) continue;
            candidates.Add(p);
        }

        var potentialParent = candidates[1];
    }

    private void TryReassembleChildren1(ArtefactPieceStateMachine[] pieces)
    {
        if (pieces == null || pieces.Length <= 2)
        {
            Debug.Log("Not enough pieces to reassemble");
            return;
        }

        // Skip index 0 kalau itu parent lama
        var candidates = new List<ArtefactPieceStateMachine>();

        // foreach (var p in pieces)
        // {
        //     if (p == null) continue;
        //     candidates.Add(p);
        // }

        // Ambil piece pertama sebagai potential parent
        var potentialParent = pieces[1]; // index 0 biasanya parent lama

        for (int i = 2; i <= pieces.Length; i++)
        {
            var child = pieces[i];

            var parentSocket = potentialParent.GetAvailableSocketFor(child.pieceId);
            var childSocket = child.GetAvailableSocketFor(potentialParent.pieceId);

            if (parentSocket != null && childSocket != null)
            {
                Debug.Log($"Reassembling {potentialParent.pieceId} <-> {child.pieceId}");
                PerformAssembly(potentialParent, child, parentSocket, childSocket);
                return;
            }
        }

        Debug.Log("No valid child-child connection found");
    }
}
