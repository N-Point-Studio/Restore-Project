using UnityEditor;
using UnityEngine;

namespace DMTools.UIGuides.Editor
{
    internal struct UIGuideSnapResult
    {
        public string horizontalGuideId;
        public string verticalGuideId;
        public Vector2 delta;
        public Vector2 canvasPoint;
        public string label;

        public bool HasSnap => horizontalGuideId != null || verticalGuideId != null;
    }

    internal static class UIGuideSnapping
    {
        private struct SnapCandidate
        {
            public bool hasValue;
            public float delta;
            public float guidePosition;
            public string guideId;
            public string label;
        }

        public static bool IsBypassModifier(Event currentEvent)
        {
            return currentEvent != null && (currentEvent.control || currentEvent.command);
        }

        public static bool TryGetSnapResult(RectTransform target, RectTransform canvasRect, UIGuideCollection collection, out UIGuideSnapResult result)
        {
            result = default;
            if (target == null || canvasRect == null || collection == null)
            {
                return false;
            }

            UIGuideCollectionSettings settings = collection.settings;
            if (settings == null || !settings.snapEnabled || settings.snapThreshold <= 0f || settings.snapMode == UIGuideSnapMode.None)
            {
                return false;
            }

            Rect targetRect = UIGuidesUtility.GetRectInCanvasSpace(target, canvasRect);
            Vector2 pivot = UIGuidesUtility.GetPivotPositionInCanvasSpace(target, canvasRect);

            SnapCandidate horizontalSnap = FindBestSnap(collection, UIGuideOrientation.Horizontal, targetRect, pivot, settings);
            SnapCandidate verticalSnap = FindBestSnap(collection, UIGuideOrientation.Vertical, targetRect, pivot, settings);

            if (!settings.snapToIntersections && horizontalSnap.hasValue && verticalSnap.hasValue)
            {
                if (Mathf.Abs(horizontalSnap.delta) <= Mathf.Abs(verticalSnap.delta))
                {
                    verticalSnap = default;
                }
                else
                {
                    horizontalSnap = default;
                }
            }

            if (!horizontalSnap.hasValue && !verticalSnap.hasValue)
            {
                return false;
            }

            result.horizontalGuideId = horizontalSnap.guideId;
            result.verticalGuideId = verticalSnap.guideId;
            result.delta = new Vector2(verticalSnap.delta, horizontalSnap.delta);
            result.canvasPoint = new Vector2(
                verticalSnap.hasValue ? verticalSnap.guidePosition : pivot.x,
                horizontalSnap.hasValue ? horizontalSnap.guidePosition : pivot.y);
            result.label = BuildLabel(horizontalSnap, verticalSnap);
            return true;
        }

        private static SnapCandidate FindBestSnap(UIGuideCollection collection, UIGuideOrientation orientation, Rect targetRect, Vector2 pivot, UIGuideCollectionSettings settings)
        {
            SnapCandidate best = default;

            for (int i = 0; i < collection.guides.Count; i++)
            {
                UIGuide guide = collection.guides[i];
                if (guide == null || !guide.visible || guide.orientation != orientation)
                {
                    continue;
                }

                if (orientation == UIGuideOrientation.Horizontal)
                {
                    TryUpdateBestSnap(ref best, guide, guide.position - pivot.y, "Pivot Y", settings.snapThreshold,
                        settings.snapMode.HasFlag(UIGuideSnapMode.Pivot));
                    TryUpdateBestSnap(ref best, guide, guide.position - targetRect.yMin, "Bottom", settings.snapThreshold,
                        settings.snapMode.HasFlag(UIGuideSnapMode.Edges));
                    TryUpdateBestSnap(ref best, guide, guide.position - targetRect.yMax, "Top", settings.snapThreshold,
                        settings.snapMode.HasFlag(UIGuideSnapMode.Edges));
                    continue;
                }

                TryUpdateBestSnap(ref best, guide, guide.position - pivot.x, "Pivot X", settings.snapThreshold,
                    settings.snapMode.HasFlag(UIGuideSnapMode.Pivot));
                TryUpdateBestSnap(ref best, guide, guide.position - targetRect.xMin, "Left", settings.snapThreshold,
                    settings.snapMode.HasFlag(UIGuideSnapMode.Edges));
                TryUpdateBestSnap(ref best, guide, guide.position - targetRect.xMax, "Right", settings.snapThreshold,
                    settings.snapMode.HasFlag(UIGuideSnapMode.Edges));
            }

            return best;
        }

        private static void TryUpdateBestSnap(ref SnapCandidate best, UIGuide guide, float delta, string label, float threshold, bool enabled)
        {
            if (!enabled)
            {
                return;
            }

            float distance = Mathf.Abs(delta);
            if (distance > threshold)
            {
                return;
            }

            if (best.hasValue && distance >= Mathf.Abs(best.delta))
            {
                return;
            }

            best.hasValue = true;
            best.delta = delta;
            best.guidePosition = guide.position;
            best.guideId = guide.id;
            best.label = label;
        }

        private static string BuildLabel(SnapCandidate horizontalSnap, SnapCandidate verticalSnap)
        {
            if (horizontalSnap.hasValue && verticalSnap.hasValue)
            {
                return $"Snap: {verticalSnap.label} + {horizontalSnap.label}";
            }

            if (horizontalSnap.hasValue)
            {
                return $"Snap: {horizontalSnap.label}";
            }

            return verticalSnap.hasValue ? $"Snap: {verticalSnap.label}" : string.Empty;
        }
    }
}
