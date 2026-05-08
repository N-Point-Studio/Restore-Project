using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BetterContentSizeFitter : ContentSizeFitter
{
    public void RefreshContent()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Refresh());
        }
    }

    private IEnumerator Refresh()
    {
        enabled = false;

        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        ForceRebuildLayoutImmediate();
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        ForceRebuildLayoutImmediate();

        enabled = true;
    }

    private void ForceRebuildLayoutImmediate()
    {
        var rect = transform as RectTransform;
        if (rect == null) return;

        // Rebuild this rect then walk up the RectTransform parents so changes propagate
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        var parent = rect.parent as RectTransform;
        while (parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            parent = parent.parent as RectTransform;
        }
    }
}
