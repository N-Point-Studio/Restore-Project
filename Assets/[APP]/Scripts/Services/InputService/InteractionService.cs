using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InteractionService : IInitializable, IDisposable, ITickable
{
    private readonly GestureService gesture;
    private readonly PointService point;
    private const float ClickThreshold = 20f; // Jarak maksimum untuk dianggap klik
    private const float ClickDuration = 0.2f; // Waktu maksimum untuk dianggap klik
    private const float HoldDuration = 0.5f;  // Waktu minimum untuk dianggap hold

    private Vector2 startPosition;
    private Vector2 lastPosition;
    private float startTime;
    private bool isDragging;
    private bool isRotating;
    private bool holdTriggered;
    private bool isPressing; // Flag baru untuk menandai jari sedang menempel

    public event Action<IInteract> OnClick;
    public event Action<IInteract, Vector3> OnDrag;
    public event Action<IInteract> OnHold;

    [Inject]
    public InteractionService(GestureService gesture, PointService point)
    {
        Debug.Log("Interaction Service Constructed");
        this.gesture = gesture;
        this.point = point;
    }

    public void Initialize()
    {
        Debug.Log("Interaction Service Initialized");

        gesture.OnPrimaryStarted += PressStart;
        gesture.OnPrimaryEnded += PressEnd;
        gesture.OnPrimaryMoved += PressMove;

        gesture.OnSecondaryStarted += SecondPressStart;
        gesture.OnSecondaryEnded += SecondPressEnd;
        gesture.OnSecondaryMoved += SecondPressMove;
    }

    public void Dispose()
    {
        gesture.OnPrimaryStarted -= PressStart;
        gesture.OnPrimaryEnded -= PressEnd;
        gesture.OnPrimaryMoved -= PressMove;

        gesture.OnSecondaryStarted -= SecondPressStart;
        gesture.OnSecondaryEnded -= SecondPressEnd;
        gesture.OnSecondaryMoved -= SecondPressMove;
    }

    public void Tick()
    {
        // Jika jari sedang menempel dan belum dianggap dragging/hold
        if (isPressing && !isDragging && !holdTriggered)
        {
            float duration = Time.time - startTime;

            if (duration >= HoldDuration)
            {
                var interactable = point.GetInteractObject();
                if (interactable is IHold holdable)
                {
                    Debug.Log("Hold Triggered via Tick");
                    holdTriggered = true;
                    holdable.OnHoldPerformed();
                    OnHold?.Invoke(point.GetInteractObject());
                }
            }
        }
    }

    private void PressStart(Vector2 position)
    {
        startPosition = position;
        lastPosition = position; // Inisialisasi lastPosition
        startTime = Time.time;
        isDragging = false;
        holdTriggered = false;
        isPressing = true;
        isRotating = false;

        point.GetInteractObject()?.OnStart();
    }

    private void PressMove(Vector2 position)
    {
        float distance = Vector2.Distance(startPosition, position);
        var interactable = point.GetInteractObject();
        if (interactable == null) return;

        // Hitung delta pergerakan jari dalam pixel
        Vector2 deltaPosition = position - lastPosition;
        lastPosition = position; // Update untuk frame berikutnya

        if (!isDragging && distance > ClickThreshold)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            // Jika objek bisa di-rotate, kirim delta pixel-nya
            if (interactable is IRotate rotatable)
            {
                // Kita modifikasi interface sedikit atau kirim lewat fungsi
                // Di sini saya asumsikan kita kirim delta via worldPos (atau buat overload)
                rotatable.OnRotatePerformed(deltaPosition);
                isRotating = true;
            }

            if (interactable is IDrag draggabe)
            {
                Vector3 worldPos = ScreenToWorld(position, interactable);
                draggabe.OnDragPerformed(worldPos);
                // OnDrag?.Invoke(point.GetInteractObject(), worldPos);
            }
        }
    }

    private void PressEnd(Vector2 position)
    {
        isPressing = false; // Jari diangkat

        float distance = Vector2.Distance(startPosition, position);
        float duration = Time.time - startTime;

        if (isDragging && !isRotating)
        {
            Vector3 worldPos = ScreenToWorld(position, point.GetInteractObject());
            OnDrag?.Invoke(point.GetInteractObject(), worldPos);
        }

        if (!isDragging && !holdTriggered && distance <= ClickThreshold && duration <= ClickDuration)
        {
            (point.GetInteractObject() as IClick)?.OnClick();
            OnClick?.Invoke(point.GetInteractObject());
        }

        point.GetInteractObject()?.OnEnd();

        isRotating = false;
        isDragging = false;
        holdTriggered = false;
    }

    //logic zoom, kalo ada jari kedua press jadi zoom
    private void SecondPressStart(Vector2 position)
    {
    }

    private void SecondPressMove(Vector2 position)
    {
    }

    private void SecondPressEnd(Vector2 position)
    {
    }

    // Helper untuk mendapatkan posisi world yang akurat
    private Vector3 ScreenToWorld(Vector2 screenPos, IInteract target)
    {
        // Kita butuh referensi transform dari objek yang diinteraksi
        // Karena target adalah interface, kita asumsikan dia adalah Component/MonoBehaviour
        if (target is Component targetComp)
        {
            Camera cam = Camera.main; // Atau ambil dari PointService jika tersedia

            // Tentukan jarak Z objek dari kamera agar pergerakan tetap di bidang yang sama
            float zDistance = cam.WorldToScreenPoint(targetComp.transform.position).z;

            Vector3 screenPosWithDepth = new Vector3(screenPos.x, screenPos.y, zDistance);
            return cam.ScreenToWorldPoint(screenPosWithDepth);
        }
        return Vector3.zero;
    }
}