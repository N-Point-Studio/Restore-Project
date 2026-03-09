using UnityEngine;

public interface ICleanObject
{
    void TryClean(Vector2 uv, Texture2D brush);
}

public class CleaningObject : MonoBehaviour, ICleanObject
{
    [SerializeField] private Texture2D dirtMaskBase;
    [SerializeField] private Material material;

    private Texture2D templateDirtMask;

    private float dirtAmountTotal;
    private float dirtAmount;

    private Vector2Int lastPaintPixelPosition;

    private void Start()
    {
        CreateTexture();
        CalculateDirtTotal();
    }

    private void CreateTexture()
    {
        templateDirtMask = new Texture2D(dirtMaskBase.width, dirtMaskBase.height);
        templateDirtMask.SetPixels(dirtMaskBase.GetPixels());
        templateDirtMask.Apply();

        material = GetComponent<Renderer>().material;
        material.SetTexture("_DirtMask", templateDirtMask);
    }

    private void CalculateDirtTotal()
    {
        Color[] pixels = dirtMaskBase.GetPixels();

        foreach (var pixel in pixels)
        {
            dirtAmountTotal += pixel.g;
        }

        dirtAmount = dirtAmountTotal;
    }

    public void TryClean(Vector2 uv, Texture2D brush)
    {
        int pixelX = (int)(uv.x * templateDirtMask.width);
        int pixelY = (int)(uv.y * templateDirtMask.height);

        Vector2Int paintPixelPosition = new Vector2Int(pixelX, pixelY);

        int paintPixelDistance =
            Mathf.Abs(paintPixelPosition.x - lastPaintPixelPosition.x) +
            Mathf.Abs(paintPixelPosition.y - lastPaintPixelPosition.y);

        int maxPaintDistance = 7;

        if (paintPixelDistance < maxPaintDistance)
            return;

        lastPaintPixelPosition = paintPixelPosition;

        int pixelOffsetX = pixelX - brush.width / 2;
        int pixelOffsetY = pixelY - brush.height / 2;

        for (int x = 0; x < brush.width; x++)
        {
            for (int y = 0; y < brush.height; y++)
            {
                Color brushPixel = brush.GetPixel(x, y);

                int targetX = pixelOffsetX + x;
                int targetY = pixelOffsetY + y;

                if (targetX < 0 || targetX >= templateDirtMask.width ||
                    targetY < 0 || targetY >= templateDirtMask.height)
                    continue;

                Color dirtPixel = templateDirtMask.GetPixel(targetX, targetY);

                float removedAmount = dirtPixel.g - (dirtPixel.g * brushPixel.g);
                dirtAmount -= removedAmount;

                templateDirtMask.SetPixel(
                    targetX,
                    targetY,
                    new Color(0, dirtPixel.g * brushPixel.g, 0)
                );
            }
        }

        templateDirtMask.Apply();
    }

    public float GetCleanProgress()
    {
        if (dirtAmountTotal <= 0)
            return 0;

        return Mathf.Clamp01(1f - (dirtAmount / dirtAmountTotal));
    }
}