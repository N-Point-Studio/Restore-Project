using UnityEngine;
using UnityEngine.UI;

public class SetImageAlphaThreshold : MonoBehaviour
{
    [SerializeField] private float threshold = 0.5f;

    private Image targetImage;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (targetImage != null)
        {
            targetImage.alphaHitTestMinimumThreshold = threshold;
        }
    }
}