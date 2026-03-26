using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
public class TextColorChanger : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    public Color[] color; 

    private void Awake()
    {
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
    }

    private void OnDestroy()
    {
        if (tmpText != null) tmpText.DOKill();
    }

    public void ChangeColor(int index)
    {
        if (tmpText != null && index >= 0 && index < color.Length)
        {
            tmpText.DOKill();
            tmpText.color = color[index];
        }
    }

    public void ChangeColorSmooth(int index, float duration)
    {
        if (tmpText != null && index >= 0 && index < color.Length)
        {
            tmpText.DOKill();
            tmpText.DOColor(color[index], duration).SetUpdate(true);
        }
    }
}