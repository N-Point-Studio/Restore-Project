using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteChanger : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;

    public Image ImageComponent => image;

    private void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    public void ChangeSprite(int index)
    {
        if (index < 0 || index >= sprites.Length)
            return;

        if (image != null)
        {
            image.sprite = sprites[index];
        }
    }

    public void ChangeSprite(Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
        }
    }
}
