using UnityEngine;
using VContainer;

public class SurfaceDetectionService
{
    private readonly Camera cam;
    private readonly int pieceLayerMask;
    public Vector3 RaycastNormal { get; private set; }
    public Vector3 RaycastPos { get; private set; } = Vector3.positiveInfinity;
    public float TipRotation { get; private set; }
    public Vector2 TextureSurface { get; private set; }
    public bool HasHit { get; private set; }

    public ICleanSurfaceObject CleanObject { get; private set; }
    public ICleanHardObject HardObject { get; private set; }
    public ICleanSurface CleanableSurface { get; private set; }
    public ICleanChunk CleanableChunk { get; private set; }

    [Inject]
    public SurfaceDetectionService(Camera cam)
    {
        this.cam = cam;
        pieceLayerMask = LayerMask.GetMask("Dirts");
    }

    public bool DetectSurface(Vector2 screenPos)
    {
        ResetDetection();

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pieceLayerMask))
            return false;

        if (hit.collider.TryGetComponent(out IClean cleanable))
        {
            HasHit = cleanable.IsCleanable();
        }

        EssentialDetecting(hit);

        CleanableChunk = hit.collider.GetComponent<ICleanChunk>();

        if (CleanableChunk != null)
        {
            return HasHit;
        }

        CleanableSurface = hit.collider.GetComponentInParent<ICleanSurface>();

        if (CleanableSurface != null && CleanableSurface.IsCleanable())
        {
            return HasHit;
        }

        return false;
    }

    private void ResetDetection()
    {
        HasHit = false;
        CleanObject = null;
        HardObject = null;
        CleanableSurface = null;
        RaycastPos = Vector3.positiveInfinity;
    }

    private void EssentialDetecting(RaycastHit hit)
    {
        RaycastNormal = hit.normal;
        RaycastPos = hit.point;

        Vector3 projectedUp = Vector3.ProjectOnPlane(Vector3.up, hit.normal);
        TipRotation = Vector3.SignedAngle(Vector3.up, projectedUp, hit.normal);

        TextureSurface = hit.textureCoord;
    }
}