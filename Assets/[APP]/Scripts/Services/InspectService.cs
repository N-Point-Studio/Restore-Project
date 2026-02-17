using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InspectService : IInitializable, IDisposable
{
    private readonly InteractionService interaction;
    private IInspectable currentInspectable;
    private readonly Transform inspectPoint;
    public IInspectable CurrentInspected => currentInspectable;
    private Transform currentTransform;

    [Inject]
    public InspectService(Transform inspectPoint, InteractionService interaction)
    {
        this.inspectPoint = inspectPoint;
        this.interaction = interaction;
    }

    public void Initialize()
    {
        interaction.OnClick += OnClickPerformed;
        interaction.OnHold += OnHoldPerformed;
        interaction.OnDrag += OnDragPerformed;
    }

    public void Dispose()
    {
        interaction.OnClick -= OnClickPerformed;
        interaction.OnHold -= OnHoldPerformed;
        interaction.OnDrag -= OnDragPerformed;
    }

    private void OnClickPerformed(IInteract interact)
    {
        var inspectable = TryGetInspectable(interact);
        if (inspectable != null)
        {
            // if (inspectable is IClick)
            Inspect(inspectable, inspectable.Transform);
        }
    }

    private void OnHoldPerformed(IInteract interact)
    {
        var inspectable = TryGetInspectable(interact);
        if (inspectable == null) return;

        // Exit hanya kalau object yang dihold adalah yang sedang diinspect
        if (inspectable == currentInspectable)
        {
            ExitInspect();
        }
    }
    private void OnDragPerformed(IInteract interact, Vector3 vector)
    {
        var inspectable = TryGetInspectable(interact);
        if (inspectable == null) return;

        if (IsNearInspectZone(inspectable.Transform))
        {
            Inspect(inspectable, inspectable.Transform);
        }
    }

    private IInspectable TryGetInspectable(IInteract interact)
    {
        if (interact is ArtefactPieceStateMachine sm)
            return sm.GetInspectable();

        return null;
    }

    public void Inspect(IInspectable inspectable, Transform targetTransform)
    {
        // Kalau yang diinspect sama dengan yang sekarang → jangan apa-apa
        if (currentInspectable == inspectable)
            return;

        // Kalau ada object lain yang sedang diinspect → exit dulu
        if (currentInspectable != null)
            ExitInspect();

        currentInspectable = inspectable;
        currentTransform = targetTransform;

        targetTransform.SetParent(inspectPoint);
        targetTransform.localPosition = Vector3.zero;
        targetTransform.localRotation = Quaternion.identity;

        inspectable.EnterInspect();
    }

    public void ExitInspect()
    {
        if (currentInspectable == null)
            return;

        currentTransform.SetParent(null);

        var temp = currentInspectable;

        currentInspectable = null;
        currentTransform = null;

        temp.ExitInspect();
    }

    public bool IsNearInspectZone(Transform target)
    {
        float distance = Vector3.Distance(target.position, inspectPoint.position);
        return distance < 1.5f;
    }
}