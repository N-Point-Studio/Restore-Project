using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;



public class FragmentService : IInitializable, IDisposable
{
    private readonly Dictionary<ArtefactPieceStateMachine, string> registry = new();
    private int totalFragments;

    public int GetTotalCount() => totalFragments;

    public void Initialize()
    {
        ArtefactPieceStateMachine.OnCreated += ArtefactCreated;
    }

    public void Dispose()
    {
        ArtefactPieceStateMachine.OnCreated -= ArtefactCreated;
    }

    private void ArtefactCreated(ArtefactPieceStateMachine machine)
    {
        Register(machine);
    }

    public void Register(ArtefactPieceStateMachine sm)
    {
        if (!registry.ContainsKey(sm))
        {
            registry.Add(sm, sm.pieceId);
            totalFragments++;
            Debug.Log($"Registered: {sm.name}. Total: {totalFragments}");
        }
    }

    public float GetAssemblyProgress()
    {
        if (totalFragments <= 1) return 1f;

        int connectedCount = 0;

        foreach (var piece in registry.Keys)
        {
            if (piece.transform.parent != null)
            {
                connectedCount++;
                Debug.Log($"{piece.name} di dalem parent kok {connectedCount}/{totalFragments} = {(float)connectedCount / totalFragments}");
            }
        }

        return (float)connectedCount / totalFragments;
    }
}