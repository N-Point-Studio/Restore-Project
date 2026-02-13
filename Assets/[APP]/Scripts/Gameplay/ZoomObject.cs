using UnityEngine;

public class ZoomObject : MonoBehaviour, IZoomable
{
    [SerializeField] private float zoomSpeed = 0.005f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private float smoothSpeed = 15f;

    private float targetScale;
    private bool isZooming;

    private void Awake()
    {
        targetScale = transform.localScale.x;
    }

    private void Update()
    {
        if (!isZooming) return;
        float current = transform.localScale.x;
        float smooth = Mathf.Lerp(current, targetScale, Time.deltaTime * smoothSpeed);
        transform.localScale = Vector3.one * smooth;
    }

    public void OnZoomStart()
    {
        isZooming = true;
        targetScale = transform.localScale.x;
    }

    public void OnZoomPerformed(float delta)
    {
        float scaleAmount = delta * zoomSpeed;
        targetScale += scaleAmount;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
    }

    public void OnZoomEnd()
    {
        isZooming = false;
    }
}