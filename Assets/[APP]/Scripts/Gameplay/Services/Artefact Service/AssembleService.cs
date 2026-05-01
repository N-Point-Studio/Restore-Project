using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AssemblyService : IInitializable, IDisposable
{
    private readonly Inspection inspectPoint;
    private readonly FragmentService fragmentService;
    private readonly TutorialService tutorialService;
    private readonly GameConfigData config;
    private readonly GameplayManager gameplayManager;

    private readonly List<IArtefactPart> currentAssembleList = new();

    [Inject]
    public AssemblyService(Inspection inspectPoint, FragmentService fragmentService, TutorialService tutorialService, GameConfigData config, GameplayManager gameplayManager)
    {
        this.inspectPoint = inspectPoint;
        this.fragmentService = fragmentService;
        this.tutorialService = tutorialService;
        this.gameplayManager = gameplayManager;
        this.config = config;
    }

    public void Initialize()
    {
        AssembleEvents.OnAssembleFinished += RecenterAssembly;
    }

    public void Dispose()
    {
        AssembleEvents.OnAssembleFinished -= RecenterAssembly;
    }

    public Transform GetInspectPoint() => inspectPoint.transform;

    public bool CanSnap(IArtefactPart checkPart, Vector3 worldPos)
    {
        Camera cam = Camera.main;

        if (IsInspectEmpty())
        {
            float distance = GetFlattenedDistance(cam, worldPos, inspectPoint.transform.position);
            return distance < config.assembleSnapDistance;
        }

        foreach (var part in currentAssembleList)
        {
            var partSocket = part.GetAvailableSocketFor(checkPart.PieceId);
            if (partSocket != null)
            {
                float distance = GetFlattenedDistance(cam, worldPos, partSocket.transform.position);
                return distance <= config.socketSnapDistance;
            }
        }

        return false;
    }

    public void TryCheckSlot(IArtefactPart checkPart, Vector3 worldPos)
    {
        Camera cam = Camera.main;

        foreach (var part in currentAssembleList)
        {
            var partSocket = part.GetAvailableSocketFor(checkPart.PieceId);
            if (partSocket != null)
            {
                var renderer = partSocket.transform.GetComponent<Renderer>();

                float distance = GetFlattenedDistance(cam, worldPos, partSocket.transform.position);

                if (distance <= config.socketSnapDistance)
                {
                    checkPart.CorrectRotation(partSocket.transform.rotation);
                }
                renderer.enabled = distance <= config.socketSnapDistance;
                return;
            }
        }
    }

    private float GetFlattenedDistance(Camera cam, Vector3 posA, Vector3 posB)
    {
        if (cam == null) return Vector3.Distance(posA, posB);

        Vector3 localA = cam.transform.InverseTransformPoint(posA);
        Vector3 localB = cam.transform.InverseTransformPoint(posB);
        localA.z = localB.z; // Flatten the depth

        return Vector3.Distance(localA, localB);
    }

    public bool TryAssemble(IArtefactPart assembleObject)
    {
        if (IsInspectEmpty())
        {
            currentAssembleList.Add(assembleObject);

            assembleObject.GetTransform().SetParent(inspectPoint.GetAssemblyRoot());
            assembleObject.OnAssembled(inspectPoint.transform);

            HideAllSockets();
            inspectPoint.SetInspectionUsage(true);
            fragmentService.ProgressUpdate();

            if (gameplayManager.isTutorialAvailable)
            {
                tutorialService.CompleteAndAdvance();
            }

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

                HideAllSockets();
                inspectPoint.SetInspectionUsage(true);
                fragmentService.ProgressUpdate();
                return true;
            }
        }
        fragmentService.ProgressUpdate();
        return false;
    }

    public void Detach(IArtefactPart part)
    {
        if (!currentAssembleList.Contains(part))
            return;

        if (currentAssembleList.Count == 0)
        {
            inspectPoint.ResetPosition();
            fragmentService.ProgressUpdate();
            inspectPoint.SetInspectionUsage(false);
            return;
        }

        if (currentAssembleList.Count == 2)
        {
            foreach (var assemble in currentAssembleList)
            {
                assemble.ReleaseSocketWith(part.PieceId);
                part.ReleaseSocketWith(assemble.PieceId);
            }

            part.GetTransform().SetParent(null);
            part.OnDetached();
            currentAssembleList.Remove(part);
            fragmentService.ProgressUpdate();
            return;
        }

        foreach (var assemble in currentAssembleList)
        {
            assemble.ReleaseSocketWith(part.PieceId);
            part.ReleaseSocketWith(assemble.PieceId);
        }

        List<IArtefactPart> toDetach = new();

        foreach (var assemble in currentAssembleList)
        {
            if (assemble.IsSlotEmpty())
            {
                toDetach.Add(assemble);
            }
        }

        foreach (var item in toDetach)
        {
            item.GetTransform().SetParent(null);
            item.OnDetached();
            currentAssembleList.Remove(item);
        }

        fragmentService.ProgressUpdate();
    }

    private Vector3 CalculateCenter()
    {
        if (currentAssembleList.Count == 0) return inspectPoint.GetAssemblyRoot().position;

        Vector3 totalCenter = Vector3.zero;

        foreach (var part in currentAssembleList)
        {
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

            partTf.DOLocalMove(targetPos, config.recenterAnimDuration)
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
}