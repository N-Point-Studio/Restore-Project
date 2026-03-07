using UnityEngine;

public class TextFontChanger : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_FontAsset[] availableFonts;
    [SerializeField] private TMPro.TextMeshProUGUI targetText;

    public void ChangeFont(int fontIndex)
    {
        if (targetText != null && availableFonts != null && fontIndex >= 0 && fontIndex < availableFonts.Length)
        {
            targetText.font = availableFonts[fontIndex];
        }
    }
}
