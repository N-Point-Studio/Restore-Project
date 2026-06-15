using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class ObjectDetectionService : IInitializable, IDisposable, ITickable
{
    private readonly Camera cam;
    private IInteractObject interactable;
    public Action<IInteractObject> OnInteractDetected;
    private readonly InputSystemService inputSystemService;

    private PlaneReference plane;
    private bool isUsed = false;
    private Vector2 mousePos;

    private float cachedDragDepth;

    [Inject]
    public ObjectDetectionService(InputSystemService inputSystemService, Camera cam, PlaneReference plane)
    {
        this.cam = cam;
        this.plane = plane;
        this.inputSystemService = inputSystemService;
    }

    public void Initialize()
    {
        InteractionEvents.OnMouseMoved += HandleMouseMove;
        inputSystemService.OnLeftPressStarted += HandlePressStarted;
        inputSystemService.OnLeftPressEnded += HandlePressEnded;
    }

    public void Dispose()
    {
        InteractionEvents.OnMouseMoved -= HandleMouseMove;
        inputSystemService.OnLeftPressStarted -= HandlePressStarted;
        inputSystemService.OnLeftPressEnded -= HandlePressEnded;
    }

    private bool IsValidPosition(Vector2 pos)
    {
        return !float.IsInfinity(pos.x) && !float.IsInfinity(pos.y) &&
               !float.IsNaN(pos.x) && !float.IsNaN(pos.y);
    }

    public void SetInteractObjectUsed(bool isUsed)
    {
        this.isUsed = isUsed;

        OnInteractDetected?.Invoke(interactable);

        if (interactable is IDragObject)
        {
            if (isUsed) CursorController.instance?.SetCursorState(CursorState.GrabClose);
            else CursorController.instance?.SetCursorState(CursorState.GrabOpen);
        }
    }

    public IInteractObject GetCurrentInteract()
    {
        if (interactable != null)
        {
            if (interactable is IDragObject) CursorController.instance?.SetCursorState(CursorState.GrabOpen);
            else CursorController.instance?.SetCursorState(CursorState.Hover);
        }
        else CursorController.instance?.SetCursorState(CursorState.DefaultRounded);

        // [BUG FIX]: Simpan dulu nilainya sebelum di-null-kan agar tidak mereturn null
        IInteractObject current = interactable;
        interactable = null;
        return current;
    }

    private void HandlePressStarted(Vector2 screenPos)
    {
        if (!IsValidPosition(screenPos) || isUsed) return;

        // Berlaku untuk PC (Klik) maupun Mobile (Tap/Touch)
        IInteractObject newTarget = null;
        if (TryRaycast(screenPos, out var hit))
        {
            hit.collider.TryGetComponent(out newTarget);
        }

        Debug.Log("Press Started " + screenPos + " Hit: " + hit.collider?.name + " NewTarget: " + newTarget);

        if (newTarget != interactable)
        {
            interactable?.OnInteractEnded();
            interactable = newTarget;
            interactable?.OnInteractDetected();
            OnInteractDetected?.Invoke(interactable);
        }
    }

    private void HandlePressEnded(Vector2 screenPos)
    {
        if (!IsValidPosition(screenPos)) return;

#if UNITY_IOS || UNITY_ANDROID
        // Khusus Mobile: Saat jari diangkat, otomatis interaksi berakhir
        // karena tidak ada konsep "hover" di touch screen.
        if (interactable != null && !isUsed)
        {
            interactable.OnInteractEnded();
            interactable = null;
            OnInteractDetected?.Invoke(null);
        }
#endif
    }

    private void HandleMouseMove(Vector2 screenPos)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!IsValidPosition(screenPos)) return;

        if (isUsed)
        {
            mousePos = screenPos;
            return;
        }

        IInteractObject newTarget = null;
        if (TryRaycast(screenPos, out var hit)) hit.collider.TryGetComponent(out newTarget);

        if (newTarget != interactable)
        {
            interactable?.OnInteractEnded();
            interactable = newTarget;
            newTarget?.OnInteractDetected();
            OnInteractDetected?.Invoke(newTarget);
        }

        if (Mouse.current != null && !Mouse.current.leftButton.isPressed && !Mouse.current.rightButton.isPressed)
        {
            if (interactable != null)
            {
                if (interactable is IDragObject) CursorController.instance?.SetCursorState(CursorState.GrabOpen);
                else CursorController.instance?.SetCursorState(CursorState.Hover);
            }
            else CursorController.instance?.SetCursorState(CursorState.DefaultRounded);
        }
        mousePos = screenPos;
#endif
    }

    public void Tick()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Tick hanya mendeteksi hover secara terus-menerus di PC/Editor.
        // Di Mobile ini dimatikan agar hemat performa (karena tidak ada mouse hover).
        if (isUsed || !IsValidPosition(mousePos)) return;

        IInteractObject newTarget = null;
        if (TryRaycast(mousePos, out var hit)) hit.collider.TryGetComponent(out newTarget);

        if (newTarget != interactable)
        {
            interactable?.OnInteractEnded();
            interactable = newTarget;
            newTarget?.OnInteractDetected();
            OnInteractDetected?.Invoke(newTarget);
        }
#endif
    }

    public bool TryRaycast(Vector2 screenPos, out RaycastHit hit)
    {
        hit = default;
        if (!IsValidPosition(screenPos)) return false;

        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit);
    }

    public void CacheDragDepth(IInteractObject target)
    {
        if (target is Component targetComp && targetComp != null)
        {
            Camera camera = this.cam != null ? this.cam : Camera.main;
            cachedDragDepth = camera.WorldToScreenPoint(targetComp.transform.position).z;
        }
    }

    public Vector3 GetCachedDragWorldPos(Vector2 screenPos)
    {
        if (!IsValidPosition(screenPos)) return Vector3.zero;

        Camera camera = this.cam != null ? this.cam : Camera.main;
        Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, cachedDragDepth);
        return camera.ScreenToWorldPoint(screenPosWithDepth);
    }

    public Vector3 ScreenToWorld(Vector2 screenPos, IInteractObject target)
    {
        if (!IsValidPosition(screenPos)) return Vector3.zero;

        if (target is Component targetComp && targetComp != null)
        {
            Camera camera = this.cam != null ? this.cam : Camera.main;
            float zDistance = camera.WorldToScreenPoint(targetComp.transform.position).z;
            Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, zDistance);
            return camera.ScreenToWorldPoint(screenPosWithDepth);
        }
        return Vector3.zero;
    }

    public Vector3 ScreenToWorld(Vector2 screenPos)
    {
        if (!IsValidPosition(screenPos)) return Vector3.zero;

        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane dragPlane = new(plane.transform.up, plane.transform.position);
        if (dragPlane.Raycast(ray, out float distance)) return ray.GetPoint(distance);
        return Vector3.zero;
    }
}