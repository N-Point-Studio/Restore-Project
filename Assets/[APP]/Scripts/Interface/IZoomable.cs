using UnityEngine;

public interface IZoomable
{
    void OnZoomStart();
    void OnZoomPerformed(float zoomDelta);
    void OnZoomEnd();
}