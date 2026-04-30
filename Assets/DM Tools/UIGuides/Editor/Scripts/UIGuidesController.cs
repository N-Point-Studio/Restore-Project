using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DMTools.UIGuides.Editor
{
    internal static class UIGuidesController
    {
        private static string selectedGuideId;

        public static string SelectedGuideId => selectedGuideId;

        public static Canvas ActiveCanvas => UIGuidesUtility.GetActiveCanvas();

        public static bool IsEnabled
        {
            get => UIGuidesPersistenceService.ToolEnabled;
            set
            {
                UIGuidesPersistenceService.ToolEnabled = value;
                SceneView.RepaintAll();
            }
        }

        public static bool TryGetActiveCollection(out Canvas canvas, out UIGuideCollection collection)
        {
            canvas = ActiveCanvas;
            collection = null;
            return canvas != null && UIGuidesPersistenceService.TryGetCollection(canvas, out collection);
        }

        public static void AddGuide(UIGuideOrientation orientation)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null || !UIGuidesUtility.TryGetCanvasRect(canvas, out RectTransform canvasRect))
            {
                return;
            }

            Rect rect = UIGuidesUtility.GetCanvasLocalRect(canvasRect);
            float position = orientation == UIGuideOrientation.Horizontal ? rect.center.y : rect.center.x;
            UIGuide guide = UIGuidesPersistenceService.AddGuide(canvas, orientation, position);
            selectedGuideId = guide.id;
            SceneView.RepaintAll();
        }

        public static void DeleteSelectedGuide()
        {
            if (!TryGetSelectedGuide(out Canvas canvas, out _))
            {
                return;
            }

            UIGuidesPersistenceService.DeleteGuide(canvas, selectedGuideId);
            selectedGuideId = null;
            SceneView.RepaintAll();
        }

        public static void ClearActiveCanvasGuides()
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            UIGuidesPersistenceService.ResetCanvasData(canvas);
            selectedGuideId = null;
            SceneView.RepaintAll();
        }

        public static void ClearSelectedGuide()
        {
            if (string.IsNullOrEmpty(selectedGuideId))
            {
                return;
            }

            selectedGuideId = null;
            SceneView.RepaintAll();
        }

        public static void SetSelectedGuide(string guideId)
        {
            if (selectedGuideId == guideId)
            {
                return;
            }

            selectedGuideId = guideId;
            SceneView.RepaintAll();
        }

        public static bool TryGetSelectedGuide(out Canvas canvas, out UIGuide guide)
        {
            guide = null;
            canvas = ActiveCanvas;
            if (canvas == null || string.IsNullOrEmpty(selectedGuideId))
            {
                return false;
            }

            if (!UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection collection))
            {
                return false;
            }

            guide = collection.guides.FirstOrDefault(item => item.id == selectedGuideId);
            if (guide != null)
            {
                return true;
            }

            selectedGuideId = null;
            return false;
        }

        public static void SetGuidesVisible(bool visible)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection) &&
                existingCollection.settings.guidesVisible == visible)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.guidesVisible = visible, visible ? "Show UI Guides" : "Hide UI Guides");
            SceneView.RepaintAll();
        }

        public static void ToggleGuidesVisible()
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            bool visible = true;
            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection collection))
            {
                visible = collection.settings.guidesVisible;
            }

            SetGuidesVisible(!visible);
        }

        public static void SetDefaultColor(Color color)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection) &&
                existingCollection.settings.defaultColor.Equals(color))
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.defaultColor = color, "Set Default UI Guide Color");
            SceneView.RepaintAll();
        }

        public static void SetCoordinateOrigin(UIGuideCoordinateOrigin origin)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection) &&
                existingCollection.settings.coordinateOrigin == origin)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.coordinateOrigin = origin, "Set UI Guide Coordinate Origin");
            SceneView.RepaintAll();
        }

        public static void SetSnapEnabled(bool enabled)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection) &&
                existingCollection.settings.snapEnabled == enabled)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.snapEnabled = enabled, enabled ? "Enable UI Guide Snapping" : "Disable UI Guide Snapping");
            SceneView.RepaintAll();
        }

        public static void SetSnapThreshold(float threshold)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            float sanitizedThreshold = Mathf.Max(0f, threshold);
            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection) &&
                Mathf.Approximately(existingCollection.settings.snapThreshold, sanitizedThreshold))
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.snapThreshold = sanitizedThreshold, "Set UI Guide Snap Threshold");
            SceneView.RepaintAll();
        }

        public static void SetSnapModeEnabled(UIGuideSnapMode mode, bool enabled)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            UIGuideSnapMode updatedMode = UIGuideSnapMode.None;
            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection))
            {
                updatedMode = existingCollection.settings.snapMode;
            }

            updatedMode = enabled ? updatedMode | mode : updatedMode & ~mode;

            if (UIGuidesPersistenceService.TryGetCollection(canvas, out existingCollection) &&
                existingCollection.settings.snapMode == updatedMode)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.snapMode = updatedMode, "Set UI Guide Snap Mode");
            SceneView.RepaintAll();
        }

        public static void SetSnapToIntersections(bool enabled)
        {
            Canvas canvas = ActiveCanvas;
            if (canvas == null)
            {
                return;
            }

            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection existingCollection) &&
                existingCollection.settings.snapToIntersections == enabled)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateCollection(canvas, collection => collection.settings.snapToIntersections = enabled, enabled ? "Enable UI Guide Intersection Snap" : "Disable UI Guide Intersection Snap");
            SceneView.RepaintAll();
        }

        public static void SetSelectedGuideColor(Color color)
        {
            if (!TryGetSelectedGuide(out Canvas canvas, out UIGuide guide))
            {
                return;
            }

            if (guide.color.Equals(color))
            {
                return;
            }

            UIGuidesPersistenceService.UpdateGuide(canvas, guide.id, item => item.color = color, "Set UI Guide Color");
            SceneView.RepaintAll();
        }

        public static void SetSelectedGuideVisible(bool visible)
        {
            if (!TryGetSelectedGuide(out Canvas canvas, out UIGuide guide))
            {
                return;
            }

            if (guide.visible == visible)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateGuide(canvas, guide.id, item => item.visible = visible, visible ? "Show UI Guide" : "Hide UI Guide");
            SceneView.RepaintAll();
        }

        public static void SetSelectedGuidePosition(float displayPosition)
        {
            if (!TryGetSelectedGuide(out Canvas canvas, out UIGuide guide))
            {
                return;
            }

            float canvasPosition = displayPosition;
            UIGuideCoordinateOrigin origin = UIGuideCoordinateOrigin.Center;
            if (UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection collection))
            {
                origin = collection.settings.coordinateOrigin;
            }

            if (UIGuidesUtility.TryGetCanvasRect(canvas, out RectTransform canvasRect))
            {
                canvasPosition = UIGuidesUtility.ToCanvasGuidePosition(canvasRect, guide.orientation, displayPosition, origin);
                canvasPosition = UIGuidesUtility.ClampGuidePosition(canvasRect, guide.orientation, canvasPosition);
            }

            if (Mathf.Approximately(guide.position, canvasPosition))
            {
                return;
            }

            UIGuidesPersistenceService.UpdateGuide(canvas, guide.id, item => item.position = canvasPosition, "Set UI Guide Position");
            SceneView.RepaintAll();
        }

        public static void SetSelectedGuideLocked(bool locked)
        {
            if (!TryGetSelectedGuide(out Canvas canvas, out UIGuide guide))
            {
                return;
            }

            if (guide.locked == locked)
            {
                return;
            }

            UIGuidesPersistenceService.UpdateGuide(canvas, guide.id, item => item.locked = locked, locked ? "Lock UI Guide" : "Unlock UI Guide");
            SceneView.RepaintAll();
        }

        public static bool BeginGuideMove(Canvas canvas, string guideId)
        {
            return UIGuidesPersistenceService.BeginMoveGuide(canvas, guideId);
        }

        public static void UpdateGuidePosition(Canvas canvas, string guideId, float position)
        {
            UIGuidesPersistenceService.SetGuidePositionWithoutUndo(canvas, guideId, position);
            SceneView.RepaintAll();
        }

        public static void EndGuideMove()
        {
            UIGuidesPersistenceService.SavePendingChanges();
            SceneView.RepaintAll();
        }

        public static void CleanupStaleData()
        {
            UIGuidesPersistenceService.RemoveMissingCollections();
        }
    }
}
