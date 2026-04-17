using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CleaningObject : MonoBehaviour
{
    public Camera cam;
    public Shader paintingShader;
    private RenderTexture paintingMap;
    private Material paintingMaterial;
    private Material paintableMaterial;
    private Renderer rend;
    private Mesh mesh;
    private CommandBuffer cb;
    private RaycastHit hit;



    void Start()
    {
        rend = GetComponent<Renderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;

        paintingMaterial = new Material(paintingShader);
        paintableMaterial = rend.material;

        paintingMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.R8);
        paintingMap.Create();

        cb = new CommandBuffer();
        cb.name = "WorldSpacePainter";

        cb.SetRenderTarget(paintingMap);
        cb.ClearRenderTarget(true, true, Color.black);
        Graphics.ExecuteCommandBuffer(cb);
        cb.Clear();

        paintableMaterial.SetTexture("_Mask", paintingMap);
        paintingMaterial.SetVector("_CameraPosition", cam.transform.position);
    }

    private void Update()
    {
        // if (Mouse.current.leftButton.isPressed)
        // {
        //     Vector2 mousePos = Mouse.current.position.ReadValue();
        //     Ray ray = cam.ScreenPointToRay(mousePos);

        //     if (Physics.Raycast(ray, out hit))
        //     {
        //         if (hit.collider.gameObject != gameObject) return;
        //         Debug.Log($"[Shader] Hit detected at {hit.point} with normal {hit.normal}");
        //         ReceivePaint(hit.point, hit.normal, 0.1f, 0.5f);
        //     }
        // }
    }

    public void ReceivePaint(Vector3 hitPoint, Vector3 hitNormal, float radius, float hardness)
    {
        paintingMaterial.SetVector("_PaintPosition", hitPoint);
        paintingMaterial.SetVector("_PaintDirection", -hitNormal);

        paintingMaterial.SetFloat("_Radius", radius);
        paintingMaterial.SetFloat("_Hardness", hardness);

        cb.Clear();
        cb.SetRenderTarget(paintingMap);
        cb.DrawMesh(mesh, transform.localToWorldMatrix, paintingMaterial, 0, 0);
        Graphics.ExecuteCommandBuffer(cb);
    }

    private void OnDestroy()
    {
        // Selalu bebaskan memori GPU saat objek hancur atau pindah scene
        if (paintingMap != null)
        {
            paintingMap.Release();
            Destroy(paintingMap);
        }

        if (cb != null)
        {
            cb.Release();
        }

        if (paintingMaterial != null)
        {
            Destroy(paintingMaterial);
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 256, 256), paintingMap, ScaleMode.ScaleToFit, false, 1);
    }
}
