using UnityEngine;
using VContainer;

public enum SurfaceDetectionType
{
    Texture,
    Mesh
}

public class SurfaceDetectionService
{
    private Vector3 raycastNormal;
    private Vector3 raycastPos;
    private float tipRotation;
    private Vector2 textureSurface;
    private bool hasHit;

    private readonly Camera cam;
    private readonly int pieceLayerMask;

    private ICleanSurfaceObject currentSurfaceDetected;
    private ICleanHardObject currentHardDetected;

    [Inject]
    public SurfaceDetectionService(Camera cam)
    {
        this.cam = cam;
        pieceLayerMask = LayerMask.GetMask("Dirts");
    }

    public bool PerformRaycast(Vector2 screenPos, SurfaceDetectionType type)
    {
        ResetDetection();

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pieceLayerMask))
            return false;

        if (hit.collider.TryGetComponent(out ICleanable cleanable))
        {
            hasHit = cleanable.IsCleanable();
        }
        EssentialDetecting(hit);

        // cek interface
        currentSurfaceDetected = hit.collider.GetComponentInParent<ICleanSurfaceObject>();
        currentHardDetected = hit.collider.GetComponent<ICleanHardObject>();

        return true;
    }

    private void ResetDetection()
    {
        hasHit = false;
        currentSurfaceDetected = null;
        currentHardDetected = null;
        raycastPos = Vector3.positiveInfinity;
    }

    private void EssentialDetecting(RaycastHit hit)
    {
        raycastNormal = hit.normal;
        raycastPos = hit.point;

        Vector3 projectedUp = Vector3.ProjectOnPlane(Vector3.up, hit.normal);
        tipRotation = Vector3.SignedAngle(Vector3.up, projectedUp, hit.normal);

        textureSurface = hit.textureCoord;
    }

    public Vector3 RaycastNormal => raycastNormal;
    public Vector3 RaycastPos => raycastPos;
    public float TipRotation => tipRotation;
    public Vector2 TextureSurface => textureSurface;

    public bool HasHit => hasHit;

    public ICleanSurfaceObject CleanObject => currentSurfaceDetected;
    public ICleanHardObject HardObject => currentHardDetected;
}
