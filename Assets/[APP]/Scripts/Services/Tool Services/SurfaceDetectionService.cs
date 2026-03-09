using UnityEngine;
using VContainer;

public enum ESurfaceDetectionType
{
    Texture,
    Mesh
}

public class SurfaceDetectionService
{
    private float raycastLength = 10f;
    private Vector3 raycastNormal;
    private Vector3 raycastPos;
    private float tipRotation;
    private Vector2 textureSurface;
    private bool hasHit;
    private readonly Camera cam;

    private ICleanObject currentSurfaceDetected;

    [Inject]
    public SurfaceDetectionService(Camera cam)
    {
        this.cam = cam;
    }

    public void PerformRaycast(Vector2 screenPos, ESurfaceDetectionType type)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out ArtefactPieceStateMachine artefact))
            {
                hasHit = artefact.state != ArtefactPieceState.Idle;
                if (artefact is ICleanObject clean) currentSurfaceDetected = clean;
                if (hasHit) EssentialDetecting(hit);
            }
            // switch (type)
            // {
            //     case ESurfaceDetectionType.Texture:
            //         break;
            //     case ESurfaceDetectionType.Mesh:
            //         break;
            // }
        }
        else
        {
            hasHit = false;
            currentSurfaceDetected = null;
            raycastPos = Vector3.positiveInfinity;
        }
    }

    public void EssentialDetecting(RaycastHit hit)
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
    public ICleanObject CleanObject => currentSurfaceDetected;
}
