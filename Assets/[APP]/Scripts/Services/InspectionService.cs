using UnityEngine;
using VContainer.Unity;

public class InspectionService
{
    private readonly Transform inspectRoot;

    private IInspectable currentInspectable;
    private Transform currentTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public InspectionService(Transform inspectRoot)
    {
        Debug.Log("Inspect here");
        this.inspectRoot = inspectRoot;
    }

    public void Inspect(IInspectable inspectable, Transform targetTransform)
    {
        if (currentInspectable != null)
            ExitInspect();

        currentInspectable = inspectable;
        currentTransform = targetTransform;

        originalPosition = targetTransform.position;
        originalRotation = targetTransform.rotation;

        targetTransform.SetParent(inspectRoot);
        targetTransform.localPosition = Vector3.zero;
        targetTransform.localRotation = Quaternion.identity;

        inspectable.EnterInspect();
    }

    public void ExitInspect()
    {
        if (currentInspectable == null) return;

        currentTransform.SetParent(null);
        currentTransform.position = originalPosition;
        currentTransform.rotation = originalRotation;

        currentInspectable.ExitInspect();

        currentInspectable = null;
        currentTransform = null;
    }
}