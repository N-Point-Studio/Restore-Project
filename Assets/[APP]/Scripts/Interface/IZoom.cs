using UnityEngine;

public interface IZoom : IInteract
{
    void OnZoomPerformed(float zoomDelta);
}