using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class TutorialLocalizationBridge : MonoBehaviour
{
    [Tooltip("The Localized String for this specific tutorial step.")]
    [SerializeField] private LocalizedString localizedText;
    
    [Tooltip("The TMP_Text component that Ninesoft is also trying to control.")]
    [SerializeField] private TMP_Text targetText;

    private string currentTranslatedText;

    private void OnEnable()
    {
        if (localizedText != null)
        {
            localizedText.StringChanged += OnStringChanged;
            localizedText.RefreshString();
        }
    }

    private void OnDisable()
    {
        if (localizedText != null)
        {
            localizedText.StringChanged -= OnStringChanged;
        }
    }

    private void OnStringChanged(string value)
    {
        currentTranslatedText = value;
        ApplyText();
    }

    private void LateUpdate()
    {
        ApplyText();
    }

    private void ApplyText()
    {
        if (targetText != null && !string.IsNullOrEmpty(currentTranslatedText))
        {
            if (targetText.text != currentTranslatedText)
            {
                targetText.text = currentTranslatedText;
            }
        }
    }
}