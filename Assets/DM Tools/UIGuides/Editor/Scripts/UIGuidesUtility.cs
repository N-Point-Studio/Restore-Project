using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DMTools.UIGuides.Editor
{
    internal static class UIGuidesUtility
    {
        private static readonly Vector3[] WorldCorners = new Vector3[4];

        public static string GetCanvasGlobalId(Canvas canvas)
        {
            return canvas == null ? null : GlobalObjectId.GetGlobalObjectIdSlow(canvas).ToString();
        }

        public static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new();
            Transform current = transform;
            while (current != null)
            {
                if (builder.Length > 0)
                {
                    builder.Insert(0, '/');
                }

                builder.Insert(0, current.name);
                current = current.parent;
            }

            return builder.ToString();
        }

        public static Canvas GetActiveCanvas()
        {
            if (Selection.activeGameObject != null)
            {
                Canvas selectedCanvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
                if (selectedCanvas != null)
                {
                    return selectedCanvas;
                }
            }

            StageHandle stageHandle = StageUtility.GetCurrentStageHandle();
            Canvas[] canvases = stageHandle.FindComponentsOfType<Canvas>();
            Canvas bestCanvas = null;

            foreach (Canvas canvas in canvases)
            {
                if (canvas == null || !canvas.isActiveAndEnabled || !canvas.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (bestCanvas == null)
                {
                    bestCanvas = canvas;
                }

                if (canvas.gameObject.scene == SceneManager.GetActiveScene())
                {
                    return canvas;
                }
            }

            return bestCanvas;
        }

        public static bool TryGetActiveSnapTarget(Canvas canvas, out RectTransform rectTransform)
        {
            rectTransform = null;
            if (canvas == null || Selection.activeGameObject == null)
            {
                return false;
            }

            rectTransform = Selection.activeGameObject.GetComponent<RectTransform>();
            if (rectTransform == null || rectTransform == canvas.transform)
            {
                rectTransform = null;
                return false;
            }

            if (!rectTransform.IsChildOf(canvas.transform) || Selection.gameObjects.Length != 1)
            {
                rectTransform = null;
                return false;
            }

            return true;
        }

        public static bool TryGetCanvasRect(Canvas canvas, out RectTransform rectTransform)
        {
            rectTransform = canvas != null ? canvas.transform as RectTransform : null;
            return rectTransform != null;
        }

        public static Rect GetCanvasLocalRect(RectTransform canvasRect)
        {
            Rect rect = canvasRect.rect;
            Vector2 pivotOffset = new(rect.width * canvasRect.pivot.x, rect.height * canvasRect.pivot.y);
            rect.position = -pivotOffset;
            return rect;
        }

        public static bool TryGetCanvasLocalPoint(RectTransform canvasRect, Vector2 guiPosition, out Vector2 localPoint)
        {
            Plane plane = new(canvasRect.forward, canvasRect.position);
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
            if (!plane.Raycast(ray, out float enter))
            {
                localPoint = default;
                return false;
            }

            Vector3 worldPoint = ray.GetPoint(enter);
            localPoint = canvasRect.InverseTransformPoint(worldPoint);
            return true;
        }

        public static Vector2 GetPivotPositionInCanvasSpace(RectTransform rectTransform, RectTransform canvasRect)
        {
            return canvasRect.InverseTransformPoint(rectTransform.position);
        }

        public static Rect GetRectInCanvasSpace(RectTransform rectTransform, RectTransform canvasRect)
        {
            rectTransform.GetWorldCorners(WorldCorners);

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            for (int i = 0; i < WorldCorners.Length; i++)
            {
                Vector3 localCorner = canvasRect.InverseTransformPoint(WorldCorners[i]);
                minX = Mathf.Min(minX, localCorner.x);
                minY = Mathf.Min(minY, localCorner.y);
                maxX = Mathf.Max(maxX, localCorner.x);
                maxY = Mathf.Max(maxY, localCorner.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public static void ApplyCanvasSpaceDelta(RectTransform rectTransform, RectTransform canvasRect, Vector2 canvasDelta)
        {
            if (rectTransform == null || canvasRect == null || canvasDelta == Vector2.zero)
            {
                return;
            }

            Vector3 worldDelta = canvasRect.TransformVector(new Vector3(canvasDelta.x, canvasDelta.y, 0f));
            rectTransform.position += worldDelta;
            EditorUtility.SetDirty(rectTransform);
        }

        public static void GetGuideEndpoints(RectTransform canvasRect, UIGuide guide, out Vector3 start, out Vector3 end)
        {
            Rect rect = GetCanvasLocalRect(canvasRect);

            if (guide.orientation == UIGuideOrientation.Horizontal)
            {
                start = new Vector3(rect.xMin, guide.position, 0f);
                end = new Vector3(rect.xMax, guide.position, 0f);
                return;
            }

            start = new Vector3(guide.position, rect.yMin, 0f);
            end = new Vector3(guide.position, rect.yMax, 0f);
        }

        public static float ClampGuidePosition(RectTransform canvasRect, UIGuideOrientation orientation, float value)
        {
            Rect rect = GetCanvasLocalRect(canvasRect);
            return orientation == UIGuideOrientation.Horizontal
                ? Mathf.Clamp(value, rect.yMin, rect.yMax)
                : Mathf.Clamp(value, rect.xMin, rect.xMax);
        }

        public static float ToDisplayGuidePosition(RectTransform canvasRect, UIGuideOrientation orientation, float canvasPosition, UIGuideCoordinateOrigin origin)
        {
            Rect rect = GetCanvasLocalRect(canvasRect);
            return orientation == UIGuideOrientation.Horizontal
                ? ToDisplayY(rect, canvasPosition, origin)
                : ToDisplayX(rect, canvasPosition, origin);
        }

        public static float ToCanvasGuidePosition(RectTransform canvasRect, UIGuideOrientation orientation, float displayPosition, UIGuideCoordinateOrigin origin)
        {
            Rect rect = GetCanvasLocalRect(canvasRect);
            return orientation == UIGuideOrientation.Horizontal
                ? ToCanvasY(rect, displayPosition, origin)
                : ToCanvasX(rect, displayPosition, origin);
        }

        public static string GetCoordinateOriginLabel(UIGuideCoordinateOrigin origin)
        {
            return origin switch
            {
                UIGuideCoordinateOrigin.TopLeft => "Top Left",
                UIGuideCoordinateOrigin.TopRight => "Top Right",
                UIGuideCoordinateOrigin.BottomLeft => "Bottom Left",
                UIGuideCoordinateOrigin.BottomRight => "Bottom Right",
                _ => "Center"
            };
        }

        public static string FormatGuideLabel(UIGuide guide, RectTransform canvasRect, UIGuideCoordinateOrigin origin)
        {
            string axis = guide.orientation == UIGuideOrientation.Horizontal ? "Y" : "X";
            float position = ToDisplayGuidePosition(canvasRect, guide.orientation, guide.position, origin);
            return $"{axis}: {position:0.#} px";
        }

        private static float ToDisplayX(Rect rect, float canvasX, UIGuideCoordinateOrigin origin)
        {
            return origin switch
            {
                UIGuideCoordinateOrigin.TopLeft or UIGuideCoordinateOrigin.BottomLeft => canvasX - rect.xMin,
                UIGuideCoordinateOrigin.TopRight or UIGuideCoordinateOrigin.BottomRight => rect.xMax - canvasX,
                _ => canvasX - rect.center.x
            };
        }

        private static float ToDisplayY(Rect rect, float canvasY, UIGuideCoordinateOrigin origin)
        {
            return origin switch
            {
                UIGuideCoordinateOrigin.TopLeft or UIGuideCoordinateOrigin.TopRight => rect.yMax - canvasY,
                UIGuideCoordinateOrigin.BottomLeft or UIGuideCoordinateOrigin.BottomRight => canvasY - rect.yMin,
                _ => canvasY - rect.center.y
            };
        }

        private static float ToCanvasX(Rect rect, float displayX, UIGuideCoordinateOrigin origin)
        {
            return origin switch
            {
                UIGuideCoordinateOrigin.TopLeft or UIGuideCoordinateOrigin.BottomLeft => rect.xMin + displayX,
                UIGuideCoordinateOrigin.TopRight or UIGuideCoordinateOrigin.BottomRight => rect.xMax - displayX,
                _ => rect.center.x + displayX
            };
        }

        private static float ToCanvasY(Rect rect, float displayY, UIGuideCoordinateOrigin origin)
        {
            return origin switch
            {
                UIGuideCoordinateOrigin.TopLeft or UIGuideCoordinateOrigin.TopRight => rect.yMax - displayY,
                UIGuideCoordinateOrigin.BottomLeft or UIGuideCoordinateOrigin.BottomRight => rect.yMin + displayY,
                _ => rect.center.y + displayY
            };
        }

        public static void MarkSceneDirty(Canvas canvas)
        {
            if (canvas != null)
            {
                EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            }
        }
    }
}
