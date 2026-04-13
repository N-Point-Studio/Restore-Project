using UnityEngine;
using UnityEngine.InputSystem;

public class MousePainter : MonoBehaviour
{
    public Camera cam;
    public Shader paintingShader;

    private RenderTexture paintingMap;
    private Material paintingMaterial;
    private Material paintableMaterial;
    private RaycastHit hit;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();

        paintingMaterial = new Material(paintingShader);
        paintingMaterial.SetColor("_Color", Color.red);

        paintableMaterial = rend.material;

        paintingMap = new RenderTexture(1024, 1024, 0);
        paintingMap.Create();

        RenderTexture.active = paintingMap;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;

        paintableMaterial.SetTexture("_PaintMask", paintingMap);
    }

    private void Update()
    {
        // ambil mouse dari Input System
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject != gameObject) return;

                Vector2 uv = hit.textureCoord;

                paintingMaterial.SetVector("_PaintPosition", uv);

                RenderTexture temp = RenderTexture.GetTemporary(paintingMap.width, paintingMap.height);

                Graphics.Blit(paintingMap, temp);
                Graphics.Blit(temp, paintingMap, paintingMaterial);

                RenderTexture.ReleaseTemporary(temp);
            }
        }
    }
}