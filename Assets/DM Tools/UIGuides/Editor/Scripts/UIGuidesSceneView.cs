using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace DMTools.UIGuides.Editor
{
    [InitializeOnLoad]
    internal static class UIGuidesSceneView
    {
        private struct GuideDrawData
        {
            public GuideDrawData(UIGuide guide, Vector3 start, Vector3 end)
            {
                Guide = guide;
                Start = start;
                End = end;
            }

            public UIGuide Guide { get; }
            public Vector3 Start { get; }
            public Vector3 End { get; }
        }

        private static readonly Dictionary<int, string> ControlToGuideId = new();
        private static readonly List<GuideDrawData> VisibleGuides = new();

        private const float GuidePickDistance = 8f;
        private const float GuideDragStartDistance = 3f;
        private static readonly Color SnapAccent = new(0.45f, 0.87f, 1f, 0.96f);

        private static GUIStyle labelStyle;
        private static GUIStyle sceneValueFieldStyle;
        private static GUIStyle shortcutStyle;
        private static GUIStyle snapStyle;

        private static int activeControlId;
        private static string draggingGuideId;
        private static string hoverGuideId;
        private static Canvas draggingCanvas;
        private static bool moveUndoRegistered;
        private static string pendingGuideId;
        private static int pendingGuideControlId;
        private static Canvas pendingGuideCanvas;
        private static Vector2 pendingGuideMousePosition;
        private static RectTransform snappingRectTransform;
        private static bool snapUndoRegistered;
        private static UIGuideSnapResult currentSnapHint;

        static UIGuidesSceneView()
        {
            SceneView.duringSceneGui += OnSceneGui;
            Selection.selectionChanged += HandleSelectionChanged;
            Undo.undoRedoPerformed += HandleUndoRedo;
            EditorApplication.playModeStateChanged += _ => ResetInteractionState();
            EditorApplication.hierarchyChanged += UIGuidesController.CleanupStaleData;
        }

        private static GUIStyle LabelStyle
        {
            get
            {
                if (labelStyle != null)
                {
                    return labelStyle;
                }

                GUIStyle baseStyle = EditorStyles.miniBoldLabel ?? GUI.skin.label;
                labelStyle = new GUIStyle(baseStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                labelStyle.normal.textColor = Color.white;
                return labelStyle;
            }
        }

        private static GUIStyle ShortcutStyle
        {
            get
            {
                if (shortcutStyle != null)
                {
                    return shortcutStyle;
                }

                shortcutStyle = new GUIStyle(EditorStyles.miniLabel ?? GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                shortcutStyle.normal.textColor = new Color(0.75f, 0.8f, 0.88f, 0.95f);
                return shortcutStyle;
            }
        }

        private static GUIStyle SceneValueFieldStyle
        {
            get
            {
                if (sceneValueFieldStyle != null)
                {
                    return sceneValueFieldStyle;
                }

                sceneValueFieldStyle = new GUIStyle(EditorStyles.numberField)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(2, 2, 0, 0)
                };
                sceneValueFieldStyle.normal.textColor = Color.white;
                sceneValueFieldStyle.focused.textColor = Color.white;
                sceneValueFieldStyle.hover.textColor = Color.white;
                sceneValueFieldStyle.active.textColor = Color.white;
                return sceneValueFieldStyle;
            }
        }

        private static GUIStyle SnapStyle
        {
            get
            {
                if (snapStyle != null)
                {
                    return snapStyle;
                }

                snapStyle = new GUIStyle(EditorStyles.miniBoldLabel ?? GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                snapStyle.normal.textColor = Color.white;
                return snapStyle;
            }
        }

        private static void HandleUndoRedo()
        {
            ResetInteractionState();
        }

        private static void HandleSelectionChanged()
        {
            ReleaseActiveControl();
            draggingGuideId = null;
            draggingCanvas = null;
            hoverGuideId = null;
            moveUndoRegistered = false;
            ClearPendingGuideDrag();
            snappingRectTransform = null;
            snapUndoRegistered = false;
            currentSnapHint = default;
            SceneView.RepaintAll();
        }

        private static void ResetInteractionState()
        {
            ReleaseActiveControl();
            HandleSelectionChanged();
        }

        private static void OnSceneGui(SceneView sceneView)
        {
            if (!UIGuidesController.IsEnabled)
            {
                ResetInteractionState();
                return;
            }

            Canvas canvas = UIGuidesController.ActiveCanvas;
            if (canvas == null || !UIGuidesPersistenceService.TryGetCollection(canvas, out UIGuideCollection collection) ||
                !UIGuidesUtility.TryGetCanvasRect(canvas, out RectTransform canvasRect))
            {
                ReleaseActiveControl();
                draggingGuideId = null;
                draggingCanvas = null;
                ClearPendingGuideDrag();
                currentSnapHint = default;
                return;
            }

            Event currentEvent = Event.current;
            ControlToGuideId.Clear();
            VisibleGuides.Clear();
            BuildVisibleGuides(collection, canvasRect);
            HandleRectTransformSnapping(currentEvent, sceneView, canvas, canvasRect, collection);

            if (collection.settings.guidesVisible)
            {
                UpdateHoverState(currentEvent, sceneView);

                foreach (GuideDrawData drawData in VisibleGuides)
                {
                    bool isSelected = drawData.Guide.id == UIGuidesController.SelectedGuideId;
                    bool isHovered = drawData.Guide.id == hoverGuideId;
                    bool isSnapTarget = drawData.Guide.id == currentSnapHint.horizontalGuideId || drawData.Guide.id == currentSnapHint.verticalGuideId;
                    DrawGuideLine(drawData.Guide, drawData.Start, drawData.End, isSelected, isHovered, isSnapTarget);
                    DrawGuideLabel(drawData.Guide, drawData.Start, canvasRect, collection.settings.coordinateOrigin, isSelected, isHovered, isSnapTarget);
                }

                HandleSelectionAndDragging(currentEvent, canvasRect);
            }
            else
            {
                hoverGuideId = null;
                ReleaseActiveControl();
                ClearPendingGuideDrag();
            }

            DrawSnapHint(canvasRect);
            HandleInlineShortcuts(currentEvent);
        }

        private static void BuildVisibleGuides(UIGuideCollection collection, RectTransform canvasRect)
        {
            for (int i = 0; i < collection.guides.Count; i++)
            {
                UIGuide guide = collection.guides[i];
                if (guide == null || !guide.visible)
                {
                    continue;
                }

                UIGuidesUtility.GetGuideEndpoints(canvasRect, guide, out Vector3 localStart, out Vector3 localEnd);
                Vector3 start = canvasRect.TransformPoint(localStart);
                Vector3 end = canvasRect.TransformPoint(localEnd);
                VisibleGuides.Add(new GuideDrawData(guide, start, end));

                if (!collection.settings.guidesVisible)
                {
                    continue;
                }

                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                ControlToGuideId[controlId] = guide.id;

                float distance = HandleUtility.DistanceToLine(start, end);
                if (distance <= GuidePickDistance)
                {
                    HandleUtility.AddControl(controlId, distance);
                }
            }
        }

        private static void DrawGuideLine(UIGuide guide, Vector3 start, Vector3 end, bool isSelected, bool isHovered, bool isSnapTarget)
        {
            Color lineColor = guide.color;
            if (isSnapTarget)
            {
                lineColor = Color.Lerp(lineColor, SnapAccent, 0.7f);
            }
            else if (isSelected)
            {
                lineColor = Color.Lerp(lineColor, Color.white, 0.48f);
            }
            else if (isHovered)
            {
                lineColor = Color.Lerp(lineColor, Color.white, 0.2f);
            }

            if (isSelected || isSnapTarget)
            {
                Handles.color = isSnapTarget ? new Color(SnapAccent.r, SnapAccent.g, SnapAccent.b, 0.2f) : new Color(1f, 1f, 1f, 0.18f);
                Handles.DrawAAPolyLine(isSnapTarget ? 8f : 7f, start, end);
            }

            Handles.color = lineColor;
            float thickness = isSelected ? 4f : isSnapTarget ? 4f : isHovered ? 3f : 2f;
            Handles.DrawAAPolyLine(thickness, start, end);
        }

        private static void DrawGuideLabel(UIGuide guide, Vector3 worldAnchor, RectTransform canvasRect, UIGuideCoordinateOrigin origin, bool isSelected, bool isHovered, bool isSnapTarget)
        {
            Handles.BeginGUI();
            Vector2 guiPosition = HandleUtility.WorldToGUIPoint(worldAnchor);
            Rect labelRect = new(guiPosition.x - 48f, guiPosition.y - 21f, 96f, 20f);
            Rect hintRect = new(guiPosition.x - 48f, guiPosition.y - 4f, 96f, 14f);

            Color background = isSnapTarget
                ? new Color(0.06f, 0.16f, 0.21f, 0.94f)
                : isSelected
                    ? new Color(0.05f, 0.09f, 0.15f, 0.94f)
                    : isHovered
                        ? new Color(0.05f, 0.05f, 0.05f, 0.72f)
                        : new Color(0f, 0f, 0f, 0.55f);

            EditorGUI.DrawRect(labelRect, background);
            if (isSelected)
            {
                DrawSelectedGuidePositionField(guide, labelRect, canvasRect, origin);
            }
            else
            {
                GUI.Label(labelRect, UIGuidesUtility.FormatGuideLabel(guide, canvasRect, origin), LabelStyle);
            }

            if (isSnapTarget)
            {
                GUI.Label(hintRect, "Snap", ShortcutStyle);
            }
            else if (isSelected)
            {
                GUI.Label(hintRect, guide.locked ? "Locked" : "Drag", ShortcutStyle);
            }
            else if (isHovered)
            {
                GUI.Label(hintRect, guide.locked ? "Locked" : "Click to select", ShortcutStyle);
            }

            Handles.EndGUI();
        }

        private static void DrawSelectedGuidePositionField(UIGuide guide, Rect labelRect, RectTransform canvasRect, UIGuideCoordinateOrigin origin)
        {
            Rect axisRect = new(labelRect.x + 5f, labelRect.y, 14f, labelRect.height);
            Rect fieldRect = new(labelRect.x + 20f, labelRect.y + 1f, 52f, labelRect.height - 2f);
            Rect unitRect = new(labelRect.x + 73f, labelRect.y, 18f, labelRect.height);

            string axis = guide.orientation == UIGuideOrientation.Horizontal ? "Y" : "X";
            GUI.Label(axisRect, axis, LabelStyle);

            float displayPosition = UIGuidesUtility.ToDisplayGuidePosition(canvasRect, guide.orientation, guide.position, origin);
            EditorGUI.BeginDisabledGroup(guide.locked);
            EditorGUI.BeginChangeCheck();
            float position = EditorGUI.FloatField(fieldRect, displayPosition, SceneValueFieldStyle);
            if (EditorGUI.EndChangeCheck())
            {
                UIGuidesController.SetSelectedGuidePosition(position);
            }
            EditorGUI.EndDisabledGroup();

            GUI.Label(unitRect, "px", LabelStyle);

            Event currentEvent = Event.current;
            Rect interactionRect = new(labelRect.x, labelRect.y, labelRect.width, labelRect.height + 14f);
            if (interactionRect.Contains(currentEvent.mousePosition) &&
                (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag || currentEvent.type == EventType.MouseUp))
            {
                currentEvent.Use();
            }
        }

        private static void DrawSnapHint(RectTransform canvasRect)
        {
            if (!currentSnapHint.HasSnap)
            {
                return;
            }

            Vector3 worldPoint = canvasRect.TransformPoint(new Vector3(currentSnapHint.canvasPoint.x, currentSnapHint.canvasPoint.y, 0f));
            float size = HandleUtility.GetHandleSize(worldPoint) * 0.08f;

            Handles.color = SnapAccent;
            Handles.DrawAAPolyLine(2.5f,
                worldPoint + Vector3.left * size,
                worldPoint + Vector3.right * size);
            Handles.DrawAAPolyLine(2.5f,
                worldPoint + Vector3.up * size,
                worldPoint + Vector3.down * size);

            Handles.BeginGUI();
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(worldPoint);
            Rect hintRect = new(guiPoint.x - 56f, guiPoint.y - 34f, 112f, 18f);
            EditorGUI.DrawRect(hintRect, new Color(0.05f, 0.18f, 0.22f, 0.92f));
            GUI.Label(hintRect, currentSnapHint.label, SnapStyle);
            Handles.EndGUI();
        }

        private static void UpdateHoverState(Event currentEvent, SceneView sceneView)
        {
            string nextHoverGuideId = null;
            if (GUIUtility.hotControl == 0 &&
                !currentEvent.alt &&
                ControlToGuideId.TryGetValue(HandleUtility.nearestControl, out string nearestGuideId))
            {
                nextHoverGuideId = nearestGuideId;
            }

            if (hoverGuideId == nextHoverGuideId)
            {
                return;
            }

            hoverGuideId = nextHoverGuideId;
            sceneView.Repaint();
        }

        private static void HandleSelectionAndDragging(Event currentEvent, RectTransform canvasRect)
        {
            int nearestControl = HandleUtility.nearestControl;

            switch (currentEvent.type)
            {
                case EventType.MouseDown when currentEvent.button == 0 && !currentEvent.alt && ControlToGuideId.TryGetValue(nearestControl, out string guideId):
                    activeControlId = nearestControl;
                    GUIUtility.hotControl = activeControlId;
                    pendingGuideControlId = nearestControl;
                    pendingGuideId = guideId;
                    pendingGuideCanvas = UIGuidesController.ActiveCanvas;
                    pendingGuideMousePosition = currentEvent.mousePosition;
                    UIGuidesController.SetSelectedGuide(guideId);
                    currentEvent.Use();
                    break;

                case EventType.MouseDown when currentEvent.button == 0 && !currentEvent.alt:
                    ClearPendingGuideDrag();
                    UIGuidesController.ClearSelectedGuide();
                    break;

                case EventType.MouseDrag when activeControlId != 0 && GUIUtility.hotControl == activeControlId && !string.IsNullOrEmpty(pendingGuideId):
                    if (Vector2.Distance(currentEvent.mousePosition, pendingGuideMousePosition) < GuideDragStartDistance)
                    {
                        break;
                    }

                    draggingGuideId = pendingGuideId;
                    draggingCanvas = pendingGuideCanvas;
                    moveUndoRegistered = false;
                    ClearPendingGuideDrag();
                    currentEvent.Use();
                    break;

                case EventType.MouseDrag when activeControlId != 0 && GUIUtility.hotControl == activeControlId && !string.IsNullOrEmpty(draggingGuideId):
                    if (draggingCanvas == null || !UIGuidesController.TryGetSelectedGuide(out _, out UIGuide guide) || guide.locked)
                    {
                        currentEvent.Use();
                        break;
                    }

                    if (!moveUndoRegistered)
                    {
                        moveUndoRegistered = UIGuidesController.BeginGuideMove(draggingCanvas, guide.id);
                    }

                    if (!UIGuidesUtility.TryGetCanvasLocalPoint(canvasRect, currentEvent.mousePosition, out Vector2 localPoint))
                    {
                        currentEvent.Use();
                        break;
                    }

                    float rawPosition = guide.orientation == UIGuideOrientation.Horizontal ? localPoint.y : localPoint.x;
                    float clampedPosition = UIGuidesUtility.ClampGuidePosition(canvasRect, guide.orientation, rawPosition);
                    UIGuidesController.UpdateGuidePosition(draggingCanvas, guide.id, clampedPosition);
                    currentEvent.Use();
                    break;

                case EventType.MouseUp when activeControlId != 0 && GUIUtility.hotControl == activeControlId:
                    if (moveUndoRegistered)
                    {
                        UIGuidesController.EndGuideMove();
                    }

                    GUIUtility.hotControl = 0;
                    activeControlId = 0;
                    draggingGuideId = null;
                    draggingCanvas = null;
                    moveUndoRegistered = false;
                    ClearPendingGuideDrag();
                    currentEvent.Use();
                    break;

                case EventType.MouseUp:
                    ClearPendingGuideDrag();
                    break;
            }
        }

        private static void HandleRectTransformSnapping(Event currentEvent, SceneView sceneView, Canvas canvas, RectTransform canvasRect, UIGuideCollection collection)
        {
            if ((activeControlId != 0 && GUIUtility.hotControl == activeControlId) || currentEvent.alt || UIGuideSnapping.IsBypassModifier(currentEvent))
            {
                currentSnapHint = default;
                if (currentEvent.type == EventType.MouseUp)
                {
                    snappingRectTransform = null;
                    snapUndoRegistered = false;
                }

                return;
            }

            if (!UIGuidesUtility.TryGetActiveSnapTarget(canvas, out RectTransform targetRect))
            {
                if (currentEvent.type == EventType.MouseUp)
                {
                    snappingRectTransform = null;
                    snapUndoRegistered = false;
                }

                currentSnapHint = default;
                return;
            }

            if (currentEvent.type == EventType.MouseDown)
            {
                if (currentEvent.button != 0)
                {
                    return;
                }

                snappingRectTransform = targetRect;
                snapUndoRegistered = false;
                currentSnapHint = default;
                return;
            }

            bool isBuiltInRectDrag = currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && GUIUtility.hotControl != 0;
            if (!isBuiltInRectDrag)
            {
                if (currentEvent.type == EventType.MouseUp)
                {
                    snappingRectTransform = null;
                    snapUndoRegistered = false;
                    currentSnapHint = default;
                }

                return;
            }

            if (snappingRectTransform != targetRect)
            {
                snappingRectTransform = targetRect;
                snapUndoRegistered = false;
            }

            if (!UIGuideSnapping.TryGetSnapResult(targetRect, canvasRect, collection, out UIGuideSnapResult snapResult))
            {
                currentSnapHint = default;
                return;
            }

            currentSnapHint = snapResult;
            if (!snapUndoRegistered)
            {
                Undo.RecordObject(targetRect, "Snap RectTransform To UI Guide");
                snapUndoRegistered = true;
            }

            UIGuidesUtility.ApplyCanvasSpaceDelta(targetRect, canvasRect, snapResult.delta);
            UIGuidesUtility.MarkSceneDirty(canvas);
            sceneView.Repaint();
        }

        private static void HandleInlineShortcuts(Event currentEvent)
        {
            if (GUIUtility.keyboardControl != 0 || currentEvent.type != EventType.KeyDown || currentEvent.alt || currentEvent.control || currentEvent.command)
            {
                return;
            }

            if (currentEvent.keyCode != KeyCode.Delete && currentEvent.keyCode != KeyCode.Backspace)
            {
                return;
            }

            if (!UIGuidesController.TryGetSelectedGuide(out _, out _))
            {
                return;
            }

            UIGuidesController.DeleteSelectedGuide();
            currentEvent.Use();
        }

        private static bool CanUseShortcuts()
        {
            return UIGuidesController.IsEnabled && UIGuidesController.ActiveCanvas != null;
        }

        private static void ClearPendingGuideDrag()
        {
            pendingGuideId = null;
            pendingGuideControlId = 0;
            pendingGuideCanvas = null;
            pendingGuideMousePosition = Vector2.zero;
        }

        private static void ReleaseActiveControl()
        {
            if (activeControlId != 0 && GUIUtility.hotControl == activeControlId)
            {
                GUIUtility.hotControl = 0;
            }

            activeControlId = 0;
        }

        [Shortcut("UI Guides/Add Horizontal Guide", typeof(SceneView), KeyCode.H, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        private static void AddHorizontalShortcut(ShortcutArguments arguments)
        {
            if (!CanUseShortcuts())
            {
                return;
            }

            UIGuidesController.AddGuide(UIGuideOrientation.Horizontal);
        }

        [Shortcut("UI Guides/Add Vertical Guide", typeof(SceneView), KeyCode.V, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        private static void AddVerticalShortcut(ShortcutArguments arguments)
        {
            if (!CanUseShortcuts())
            {
                return;
            }

            UIGuidesController.AddGuide(UIGuideOrientation.Vertical);
        }

        [Shortcut("UI Guides/Delete Selected Guide", typeof(SceneView), KeyCode.Backspace, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        private static void DeleteSelectedShortcut(ShortcutArguments arguments)
        {
            if (!CanUseShortcuts() || !UIGuidesController.TryGetSelectedGuide(out _, out _))
            {
                return;
            }

            UIGuidesController.DeleteSelectedGuide();
        }

        [Shortcut("UI Guides/Toggle Guide Visibility", typeof(SceneView), KeyCode.G, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        private static void ToggleVisibilityShortcut(ShortcutArguments arguments)
        {
            if (!CanUseShortcuts())
            {
                return;
            }

            UIGuidesController.ToggleGuidesVisible();
        }
    }
}




