using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clean : MonoBehaviour
{
    [Header("Textures")]
    public Texture2D _dirtMaskBase;
    public Material _material;
    private Texture2D _templateDirtMask;

    [Header("Progress (0 = kotor, 1 = bersih)")]
    [Range(0, 1f)]
    [SerializeField] private float progress = 0f;

    private float dirtAmountTotal = 0f;
    private float dirtAmount = 0f;
    private Vector2Int lastPaintPixelPosition;


    private void Start()
    {
        CreateTexture();
        StartCoroutine(CalculateDirtTotalAsync());
    }


    private IEnumerator CalculateDirtTotalAsync()
    {
        dirtAmountTotal = 0f;

        Color[] pixels = _dirtMaskBase.GetPixels();
        const int batchSize = 10000;

        for (int i = 0; i < pixels.Length; i++)
        {
            dirtAmountTotal += pixels[i].g;

            if (i % batchSize == 0)
                yield return null;
        }

        dirtAmount = dirtAmountTotal;
        CleanManager.Instance.Register(this);

        Debug.Log($"{name} Dirt Total Selesai: {dirtAmountTotal}");
    }


    public bool CleaningAtPoint(Vector2 uv, Texture2D brush)
    {
        bool didCleanAnything = false;

        int pixelX = (int)(uv.x * _templateDirtMask.width);
        int pixelY = (int)(uv.y * _templateDirtMask.height);

        Debug.Log("Cleaning at pixel: " + pixelX + ", " + pixelY);

        Vector2Int paintPixelPosition = new Vector2Int(pixelX, pixelY);
        Debug.Log("Position pixel: " + paintPixelPosition);


        int paintPixelDistance = Mathf.Abs(paintPixelPosition.x - lastPaintPixelPosition.x) + Mathf.Abs(paintPixelPosition.y - lastPaintPixelPosition.y);
        int maxPaintDistance = 7;

        if (paintPixelDistance < maxPaintDistance)
        {
            Debug.Log("TisActivelyCleaning paint: " + paintPixelDistance);
            return false;
        }

        lastPaintPixelPosition = paintPixelPosition;

        int pixelOffsetX = pixelX - brush.width / 2;
        int pixelOffsetY = pixelY - brush.height / 2;

        for (int x = 0; x < brush.width; x++)
        {
            for (int y = 0; y < brush.height; y++)
            {
                Color pixelDirt = brush.GetPixel(x, y);
                Color pixelDirtMask = _templateDirtMask.GetPixel(pixelOffsetX + x, pixelOffsetY + y);

                float removedAmount = pixelDirtMask.g - (pixelDirtMask.g * pixelDirt.g);
                dirtAmount -= removedAmount;

                _templateDirtMask.SetPixel(
                    pixelOffsetX + x,
                    pixelOffsetY + y,
                    new Color(0, pixelDirtMask.g * pixelDirt.g, 0)
                );
                didCleanAnything = true;
            }
        }

        if (didCleanAnything)
        {
            _templateDirtMask.Apply();
            UpdateProgress();
        }

        return didCleanAnything;
    }


    private void CreateTexture()
    {
        _templateDirtMask = new Texture2D(_dirtMaskBase.width, _dirtMaskBase.height);
        _templateDirtMask.SetPixels(_dirtMaskBase.GetPixels());
        _templateDirtMask.Apply();

        _material = GetComponent<Renderer>().material;
        _material.SetTexture("_DirtMask", _templateDirtMask);
    }


    private void UpdateProgress()
    {
        if (dirtAmountTotal <= 0)
            progress = 0f;
        else
            progress = Mathf.Clamp01(1f - (dirtAmount / dirtAmountTotal));
    }


    public float GetDirtAmount()
    {
        UpdateProgress(); // supaya inspector selalu update
        return progress;
    }
}
