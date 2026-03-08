using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LoadingType
{
    ProgressBar,
    Music,
    Home,
    Camera
}

public class ProjectLoadingView : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    
    [Header("Progress Bar Group")]
    [SerializeField] private GameObject progressBarGroup;
    [SerializeField] private Image loadingFill;
    
    [Header("Icon Transition Group")]
    [SerializeField] private GameObject iconGroup;
    [SerializeField] private GameObject musicObject;
    [SerializeField] private GameObject homeObject;
    [SerializeField] private GameObject cameraObject;

    private string baseLoadingText = "Loading";
    private Coroutine loadingTextAnimation;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        loadingPanel.SetActive(false);
    }

    public void ShowLoading(string text, LoadingType type)
    {
        loadingPanel.SetActive(true);
        SetupVisual(type);
        
        if (text.EndsWith("...")) baseLoadingText = text.Substring(0, text.Length - 3);
        else if (text.EndsWith("..")) baseLoadingText = text.Substring(0, text.Length - 2);
        else if (text.EndsWith(".")) baseLoadingText = text.Substring(0, text.Length - 1);
        else baseLoadingText = text;
        
        if (loadingTextAnimation != null) StopCoroutine(loadingTextAnimation);
        loadingTextAnimation = StartCoroutine(AnimateLoadingText());
        
        LoadingEvents.OnLoadingStarted?.Invoke();
    }

    private void SetupVisual(LoadingType type)
    {
        progressBarGroup.SetActive(type == LoadingType.ProgressBar);
        iconGroup.SetActive(type != LoadingType.ProgressBar);

        musicObject.SetActive(false); 
        homeObject.SetActive(false); 
        cameraObject.SetActive(false); 

        switch (type)
        {
            case LoadingType.Music:
                musicObject.SetActive(true); 
                break;
            case LoadingType.Home:
                homeObject.SetActive(true); 
                break;
            case LoadingType.Camera:
                cameraObject.SetActive(true); 
                break;
        }
    }

    public void SetProgress(float progress) => loadingFill.fillAmount = progress;
    public void HideLoading() => loadingPanel.SetActive(false);

    private IEnumerator AnimateLoadingText()
    {
        string[] dotStates = { "", ".", "..", "..." };
        int i = 0;
        while (true)
        {
            loadingText.text = baseLoadingText + dotStates[i];
            i = (i + 1) % dotStates.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }
}