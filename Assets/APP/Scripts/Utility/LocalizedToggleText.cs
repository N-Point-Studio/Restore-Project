using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;

[RequireComponent(typeof(Toggle))]
public class LocalizedToggleText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text targetText; 

    [Header("Localized Strings")]
    [SerializeField] private LocalizedString onStateString;
    [SerializeField] private LocalizedString offStateString;

    private Toggle myToggle;
    
    private string cachedOnText = "";
    private string cachedOffText = "";

    private void Awake()
    {
        myToggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        onStateString.StringChanged += OnUpdateOnText;
        offStateString.StringChanged += OnUpdateOffText;
        myToggle.onValueChanged.AddListener(OnToggleStateChanged);
    }

    private void OnDisable()
    {
        onStateString.StringChanged -= OnUpdateOnText;
        offStateString.StringChanged -= OnUpdateOffText;
        myToggle.onValueChanged.RemoveListener(OnToggleStateChanged);
    }

    private void OnUpdateOnText(string translatedText)
    {
        cachedOnText = translatedText;
        if (myToggle.isOn && targetText != null) targetText.text = cachedOnText;
    }

    private void OnUpdateOffText(string translatedText)
    {
        cachedOffText = translatedText;
        if (!myToggle.isOn && targetText != null) targetText.text = cachedOffText;
    }


    private void OnToggleStateChanged(bool isOn)
    {
        if (targetText == null) return;
        targetText.text = isOn ? cachedOnText : cachedOffText;
    }
}