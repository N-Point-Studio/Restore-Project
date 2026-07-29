using UnityEngine;
using UnityEngine.UI;

public class BetterContentSizeFitter : ContentSizeFitter
{
    public void RefreshContent()
    {
        if (!gameObject.activeInHierarchy) return;

        RectTransform rect = transform as RectTransform;
        if (rect == null) return;

        LayoutRebuilder.MarkLayoutForRebuild(rect);

        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(parentRect);
        }
    }
}