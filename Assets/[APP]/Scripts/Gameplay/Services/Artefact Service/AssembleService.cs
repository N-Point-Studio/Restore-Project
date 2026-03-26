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

    private readonly List<IArtefactPart> currentAssembleList = new();
    private float socketSnapDistance = 1f;

    [Inject]
    public AssemblyService(Inspection inspectPoint, FragmentService fragmentService, TutorialService tutorialService)
    {
        this.inspectPoint = inspectPoint;
        this.fragmentService = fragmentService;
        this.tutorialService = tutorialService;
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

    public void TryCheckSlot(IArtefactPart checkPart, Vector3 worldPos)
    {
        foreach (var part in currentAssembleList)
        {
            var partSocket = part.GetAvailableSocketFor(checkPart.PieceId);
            if (partSocket != null)
            {
                var renderer = partSocket.transform.GetComponent<Renderer>();
                float distance = Vector3.Distance(worldPos, partSocket.transform.position);

                if (distance <= socketSnapDistance)
                {
                    checkPart.CorrectRotation(partSocket.transform.rotation);
                }
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
            inspectPoint.SetInspectionUsage(true);
            fragmentService.ProgressUpdate();
            // RecenterAssembly();

            tutorialService.CompleteTutorial(TutorialIDs.DRAG_TO_INSPECT, 0, 0);
            tutorialService.StartTutorial(TutorialIDs.ZOOM_INSPECT, 0, 1);

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
        if (currentAssembleList.Contains(part))
        {
            foreach (var assemble in currentAssembleList)
            {
                assemble.ReleaseSocketWith(part.PieceId);
                part.ReleaseSocketWith(assemble.PieceId);
            }

            part.GetTransform().SetParent(null);
            part.OnDetached();
            fragmentService.ProgressUpdate();
            currentAssembleList.Remove(part);
        }
        if (currentAssembleList.Count == 0)
        {
            inspectPoint.ResetPosition();
            fragmentService.ProgressUpdate();
            inspectPoint.SetInspectionUsage(false);
        }
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
            Debug.Log("Recenter " + part);
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
}