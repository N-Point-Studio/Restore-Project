using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class FragmentService : IInitializable, IDisposable
{
    private readonly HashSet<ArtefactPieceStateMachine> registry = new();

    public void Initialize() => ArtefactPieceStateMachine.OnCreated += Register;
    public void Dispose() => ArtefactPieceStateMachine.OnCreated -= Register;

    public void Register(ArtefactPieceStateMachine sm)
    {
        if (registry.Add(sm)) Debug.Log($"Registered: {sm.name}");
    }

    public float GetAssemblyProgress()
    {
        if (registry.Count == 0) return 0f;

        int connectedPieces = 0;

        foreach (var piece in registry)
        {
            bool isAttached = false;
            foreach (var socket in piece.sockets)
            {
                if (socket.isOccupied)
                {
                    isAttached = true;
                    break;
                }
            }

            if (isAttached)
            {
                connectedPieces++;
            }
        }

        float progress = (float)connectedPieces / registry.Count;
        Debug.Log($"Progress: {connectedPieces}/{registry.Count} = {progress}");

        return progress;
    }
}