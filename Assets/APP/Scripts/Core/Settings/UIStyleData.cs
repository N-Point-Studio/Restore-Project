using System;
using UnityEngine;

public enum TypographyStyle 
{ 
    PrimaryButton, 
    SecondaryButton, 
    BodyText, 
    Header 
}

[Serializable]
public struct TypographyProfile
{
    public float desktopSize;
    public float mobileSize;
}

[CreateAssetMenu(fileName = "UIStyleData", menuName = "App/Data/UI Style Data")]
public class UIStyleData : ScriptableObject
{
    private static UIStyleData _instance;
    public static UIStyleData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<UIStyleData>("UIStyleData");
                if (_instance == null)
                {
                    Debug.LogError("[UIStyleData] Not found! Please create a UIStyleData asset inside a 'Resources' folder.");
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        _instance = null;
    }

    [Header("Typography Scales (Base 8 recommended)")]
    public TypographyProfile primaryButton = new TypographyProfile { desktopSize = 48f, mobileSize = 64f };
    public TypographyProfile secondaryButton = new TypographyProfile { desktopSize = 36f, mobileSize = 48f };
    public TypographyProfile bodyText = new TypographyProfile { desktopSize = 24f, mobileSize = 32f };
    public TypographyProfile headerText = new TypographyProfile { desktopSize = 56f, mobileSize = 72f };

    public TypographyProfile GetProfile(TypographyStyle style)
    {
        switch (style)
        {
            case TypographyStyle.PrimaryButton: return primaryButton;
            case TypographyStyle.SecondaryButton: return secondaryButton;
            case TypographyStyle.BodyText: return bodyText;
            case TypographyStyle.Header: return headerText;
            default: return bodyText;
        }
    }
}