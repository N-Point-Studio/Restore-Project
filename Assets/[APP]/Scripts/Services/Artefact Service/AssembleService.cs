using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AssemblyService : IInitializable, IDisposable
{
    private readonly FragmentService registry;
    private readonly Inspection inspectPoint;
    private readonly List<IArtefactPart> currentAssembleList = new();
    private float socketSnapDistance = 1f;

    [Inject]
    public AssemblyService(FragmentService registry, Inspection inspectPoint)
    {
        this.registry = registry;
        this.inspectPoint = inspectPoint;
    }

    public Transform GetInspectPoint() => inspectPoint.transform;

    // public void TryCheckSlot(IArtefactPart checkPart, Vector3 worldPos)
    // {
    //     foreach (var part in currentAssembleList)
    //     {
    //         var partSocket = part.GetAvailableSocketFor(checkPart.PieceId);

    //         if (partSocket == null)
    //             continue;

    //         var renderer = partSocket.transform.GetComponent<Renderer>();
    //         if (renderer == null) continue;

    //         float distance = Vector3.Distance(worldPos, partSocket.transform.position);

    //         renderer.enabled = distance <= socketSnapDistance;
    //     }
    // }

    public void TryCheckSlot(IArtefactPart checkPart, Vector3 worldPos)
    {
        foreach (var part in currentAssembleList)
        {
            var partSocket = part.GetAvailableSocketFor(checkPart.PieceId);
            if (partSocket != null)
            {
                var renderer = partSocket.transform.GetComponent<Renderer>();
                float distance = Vector3.Distance(worldPos, partSocket.transform.position);

                renderer.enabled = distance <= socketSnapDistance;
                return;
            }
        }
    }

    public bool TryAssemble(IArtefactPart assembleObject)
    {
        if (IsInspectEmpty())
        {
            currentAssembleList.Add(assembleObject);

            assembleObject.GetTransform().SetParent(inspectPoint.GetAssemblyRoot());
            assembleObject.OnAssembled(inspectPoint.transform);

            HideAllSockets();

            return true;
        }
        else
        {
            Transform tempTf = null;
            foreach (var part in currentAssembleList)
            {
                var currentSocket = assembleObject.GetAvailableSocketFor(part.PieceId);
                var partSocket = part.GetAvailableSocketFor(assembleObject.PieceId);
                if (currentSocket != null && partSocket != null)
                {
                    currentSocket.isOccupied = true;
                    partSocket.isOccupied = true;
                    tempTf = partSocket.transform;
                }
            }

            if (tempTf != null)
            {
                currentAssembleList.Add(assembleObject);

                assembleObject.GetTransform().SetParent(inspectPoint.GetAssemblyRoot());
                assembleObject.OnAssembled(tempTf);

                HideAllSockets(); // 🔥 matikan indikator

                LogProgress("Assembled", assembleObject.PieceId);

                return true;
            }
        }
        return false;
    }

    public void Detach(IArtefactPart part)
    {
        if (currentAssembleList.Contains(part))
        {
            foreach (var assemble in currentAssembleList)
            {
                assemble.ReleaseSocketWith(part.PieceId);
                part.ReleaseSocketWith(assemble.PieceId);
            }

            part.GetTransform().SetParent(null);
            part.OnDetached();
            currentAssembleList.Remove(part);
        }

        LogProgress("Detached", part.PieceId);
        if (currentAssembleList.Count == 0) { inspectPoint.ResetPosition(); }
    }

    private Vector3 CalculateCenter()
    {
        if (currentAssembleList.Count == 0) return inspectPoint.GetAssemblyRoot().position;

        Vector3 totalCenter = Vector3.zero;

        foreach (var part in currentAssembleList)
        {
            Debug.Log($"{part} is {part.GetTransform().position}");
            totalCenter += part.GetTransform().localPosition;
        }

        return totalCenter / currentAssembleList.Count;
    }

    private void RecenterAssembly()
    {
        Vector3 currentCenter = CalculateCenter();

        Vector3 offset = currentCenter;

        foreach (var part in currentAssembleList)
        {
            Transform partTf = part.GetTransform();
            Vector3 targetPos = partTf.localPosition - offset;

            partTf.DOLocalMove(targetPos, 0.5f)
                  .SetEase(Ease.OutCubic)
                  .SetLink(partTf.gameObject);
        }
    }

    private void HideAllSockets()
    {
        foreach (var part in currentAssembleList)
        {
            var sockets = part.GetSockets();

            foreach (var socket in sockets)
            {
                var renderer = socket.transform.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.enabled = false;
            }
        }
    }

    public bool IsInspectEmpty()
    {
        return currentAssembleList.Count == 0;
    }

    private void LogProgress(string action, string id)
    {
        float progress = registry.GetAssemblyProgress();
        Debug.Log($"<color=cyan>[{action}]</color> {id}. Progress: {progress * 100:F0}%");
        registry.ProgressUpdate();
    }

    public void Initialize()
    {
        InteractionEvents.OnAssembleInteractionFinished += RecenterAssembly;
    }

    public void Dispose()
    {
        InteractionEvents.OnAssembleInteractionFinished -= RecenterAssembly;
    }
}