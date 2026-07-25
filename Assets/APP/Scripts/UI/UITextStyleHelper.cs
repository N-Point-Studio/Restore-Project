using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UITextStyleHelper : MonoBehaviour
{
    [Header("Platform Typography")]
    [Tooltip("Select the style rule from UIStyleData to apply to this text.")]
    [SerializeField] private TypographyStyle textStyle = TypographyStyle.BodyText;
    
    private TMP_Text textComponent;

    private void Awake()
    {
        ApplyStyle();
    }

    /// <summary>
    /// Grabs the rule from our Singleton DB and applies the correct font size.
    /// </summary>
    public void ApplyStyle()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        if (textComponent != null)
        {
            bool isMobile = Application.isMobilePlatform;
#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            isMobile = true;
#endif
            // Fetch the profile from our Resource DB
            TypographyProfile profile = UIStyleData.Instance.GetProfile(textStyle);
            
            // Apply the size
            textComponent.fontSize = isMobile ? profile.mobileSize : profile.desktopSize;
        }
    }

    /// <summary>
    /// Call this if you ever need to change the style of a text element dynamically via code.
    /// </summary>
    public void SetStyle(TypographyStyle newStyle)
    {
        textStyle = newStyle;
        ApplyStyle();
    }
}