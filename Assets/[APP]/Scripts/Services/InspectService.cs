using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class InspectService
{
    private readonly Inspection inspectPoint;
    private IInspectable currentInspect;
    private List<IInspectable> currentInspectList = new();
    private List<IArtefactPart> currentAssembleList = new();

    private Transform originalParent;

    public event Action<IAssemble> OnInspectListAdded;
    public event Action<IAssemble> OnInspectListRemoved;

    [Inject]
    public InspectService(Inspection inspectPoint)
    {
        this.inspectPoint = inspectPoint;
    }

    public IInspectable GetCurrentInspected() => currentInspect;
    public Transform GetInspectPoint() => inspectPoint.transform;

    public void Inspect(IInspectable inspectable)
    {
        // if (currentInspect == inspectable) return;

        // if (currentInspect != null)
        // {
        //     ExitInspect();
        // }

        // currentInspect = inspectable;
        // originalParent = inspectable.GetTransform().parent;

        // inspectPoint.ResetPosition();
        if (currentInspectList.Count == 0)
        {
            if (!currentInspectList.Contains(inspectable))
            {
                currentInspectList.Add(inspectable);
                inspectable.GetTransform().SetParent(inspectPoint.transform);
                inspectable.EnterInspect(inspectPoint.transform);

                // Debug.Log("barang adalah IAssemble " + (inspectable is IAssemble));
                if (inspectable is IAssemble assemble) OnInspectListAdded?.Invoke(assemble);
            }
        }
    }

    private void AddList(IInspectable inspectable)
    {
        if (!currentInspectList.Contains(inspectable))
        {
            currentInspectList.Add(inspectable);
            inspectable.GetTransform().SetParent(inspectPoint.transform);
            inspectable.EnterInspect(inspectPoint.transform);

            // Debug.Log("barang adalah IAssemble " + (inspectable is IAssemble));
            if (inspectable is IAssemble assemble)
            {
                assemble.OnAssembled(inspectPoint.transform);
                OnInspectListAdded?.Invoke(assemble);
            }
        }
    }

    public bool TryInspect(IArtefactPart assembleObject)
    {
        if (IsInspectEmpty())
        {
            currentAssembleList.Add(assembleObject);
            assembleObject.GetTransform().SetParent(inspectPoint.transform);
            // if (assembleObject is IAssemble assemble)
            // {
            assembleObject.OnAssembled(inspectPoint.transform);
            // OnInspectListAdded?.Invoke(assembleObject);
            // }
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
                assembleObject.GetTransform().SetParent(inspectPoint.transform);
                assembleObject.OnAssembled(tempTf);
                return true;
            }
        }
        return false;
    }

    public void ExitInspect(IArtefactPart part)
    {
        if (currentAssembleList.Contains(part))
        {
            foreach (var assemble in currentAssembleList)
            {
                assemble.ReleaseSocketWith(part.PieceId);
                part.ReleaseSocketWith(assemble.PieceId);

                // var currentSocket = part.GetAvailableSocketFor(assemble.PieceId);
                // var partSocket = assemble.GetAvailableSocketFor(part.PieceId);
                // if (currentSocket != null && partSocket != null)
                // {
                //     currentSocket.isOccupied = false;
                //     partSocket.isOccupied = false;
                // }
            }

            part.GetTransform().SetParent(null);
            part.OnDetached();
            currentAssembleList.Remove(part);
        }

        if (currentAssembleList.Count == 0) { inspectPoint.ResetPosition(); }
    }

    public void ExitInspect(IInspectable inspectable)
    {
        // if (currentInspect == null) return;

        // currentInspect.GetTransform().SetParent(null, true);
        // currentInspect.ExitInspect();

        // currentInspect = null;
        // originalParent = null;

        // inspectPoint.ResetPosition();

        if (currentInspectList.Contains(inspectable))
        {
            currentInspectList.Remove(inspectable);
            inspectable.GetTransform().SetParent(null, true);
            inspectable.ExitInspect();

            if (inspectable is IAssemble assemble) OnInspectListRemoved?.Invoke(assemble);
        }

        if (currentInspectList.Count == 0) { inspectPoint.ResetPosition(); }
    }

    public bool IsInspectEmpty()
    {
        // return currentInspectList.Count == 0;
        return currentAssembleList.Count == 0;
    }
}