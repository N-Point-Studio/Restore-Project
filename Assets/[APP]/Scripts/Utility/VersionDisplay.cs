using TMPro;
using UnityEngine;

public class VersionDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text textVersion;

    private void Awake()
    {
        textVersion.text = $"Version: {Application.version}";
    }
}
