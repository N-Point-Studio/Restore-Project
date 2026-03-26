using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class SpriteColorChanger : MonoBehaviour
{
    [SerializeField] private Image image;
    public Color[] color;

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
    }

    private void OnDestroy()
    {
        if (image != null) image.DOKill();
    }

    public void ChangeColor(int index)
    {
        if (image != null && index >= 0 && index < color.Length)
        {
            image.DOKill();
            image.color = color[index];
        }
    }

    public void ChangeColorSmooth(int index, float duration)
    {
        if (image != null && index >= 0 && index < color.Length)
        {
            image.DOKill();
            image.DOColor(color[index], duration).SetUpdate(true);
        }
    }
}