using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Image))]
[RequireComponent(typeof(UnityEngine.UI.Outline))]
public class SpriteAndOutlineColorChanger : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private UnityEngine.UI.Outline outline;
    [SerializeField] private SpriteOutlineSettings[] settings;

    public Image ImageComponent => image;
    public UnityEngine.UI.Outline OutlineComponent => outline;

    private void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    public void ChangeColor(int index)
    {
        if (index < 0 || index >= settings.Length)
            return;

        if (image != null)
        {
            image.color = settings[index].imageColor;
        }

        if (outline != null)
        {
            outline.effectColor = settings[index].outlineColor;
        }
    }

    [Serializable]
    private struct SpriteOutlineSettings
    {
        public Color imageColor;
        public Color outlineColor;
    }
}