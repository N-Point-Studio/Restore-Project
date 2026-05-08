using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DMTools.UIGuides.Editor
{
    [Overlay(typeof(SceneView), "UI Guides", true)]
    internal sealed class UIGuidesOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            return new UIGuidesOverlayPanel();
        }
    }

    internal sealed class UIGuidesOverlayPanel : VisualElement
    {
        private readonly Label titleLabel;
        private readonly Label hintLabel;
        private readonly Toggle enabledToggle;
        private readonly Toggle visibleToggle;
        private readonly ColorField defaultColorField;
        private readonly PopupField<string> coordinateOriginField;
        private readonly Toggle snapEnabledToggle;
        private readonly FloatField snapThresholdField;
        private readonly Toggle snapPivotToggle;
        private readonly Toggle snapEdgesToggle;
        private readonly Toggle snapIntersectionsToggle;
        private readonly Label shortcutsLabel;
        private readonly Button addHorizontalButton;
        private readonly Button addVerticalButton;
        private readonly Button deleteButton;
        private readonly Button clearButton;
        private readonly Label selectionLabel;
        private readonly FloatField selectedPositionField;
        private readonly Toggle selectedVisibleToggle;
        private readonly Toggle selectedLockedToggle;
        private readonly ColorField selectedColorField;

        private static readonly List<string> CoordinateOriginChoices = new()
        {
            "Center",
            "Top Left",
            "Top Right",
            "Bottom Left",
            "Bottom Right"
        };

        public UIGuidesOverlayPanel()
        {
            style.minWidth = 248f;
            style.paddingLeft = 8f;
            style.paddingRight = 8f;
            style.paddingTop = 8f;
            style.paddingBottom = 8f;

            titleLabel = new Label();
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 4f;
            Add(titleLabel);

            hintLabel = new Label();
            hintLabel.style.whiteSpace = WhiteSpace.Normal;
            hintLabel.style.fontSize = 11f;
            hintLabel.style.color = new StyleColor(new Color(0.78f, 0.82f, 0.88f));
            hintLabel.style.marginBottom = 6f;
            Add(hintLabel);

            enabledToggle = new Toggle("Tool Enabled");
            enabledToggle.style.marginBottom = 4f;
            enabledToggle.RegisterValueChangedCallback(evt => UIGuidesController.IsEnabled = evt.newValue);
            Add(enabledToggle);

            visibleToggle = new Toggle("Show Guides");
            visibleToggle.style.marginBottom = 4f;
            visibleToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetGuidesVisible(evt.newValue));
            Add(visibleToggle);

            defaultColorField = new ColorField("Default Color");
            defaultColorField.showAlpha = true;
            defaultColorField.style.marginBottom = 6f;
            defaultColorField.RegisterValueChangedCallback(evt => UIGuidesController.SetDefaultColor(evt.newValue));
            Add(defaultColorField);

            coordinateOriginField = new PopupField<string>("Coordinate Origin", CoordinateOriginChoices, 0);
            coordinateOriginField.style.marginBottom = 6f;
            coordinateOriginField.RegisterValueChangedCallback(evt => UIGuidesController.SetCoordinateOrigin(ToCoordinateOrigin(evt.newValue)));
            Add(coordinateOriginField);

            Label snapHeader = new("Snapping")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 2f,
                    marginBottom = 4f
                }
            };
            Add(snapHeader);

            snapEnabledToggle = new Toggle("Enable Snap");
            snapEnabledToggle.style.marginBottom = 4f;
            snapEnabledToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetSnapEnabled(evt.newValue));
            Add(snapEnabledToggle);

            snapThresholdField = new FloatField("Threshold")
            {
                value = 8f
            };
            snapThresholdField.style.marginBottom = 4f;
            snapThresholdField.RegisterValueChangedCallback(evt => UIGuidesController.SetSnapThreshold(evt.newValue));
            Add(snapThresholdField);

            snapPivotToggle = new Toggle("Snap Pivot");
            snapPivotToggle.style.marginBottom = 2f;
            snapPivotToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetSnapModeEnabled(UIGuideSnapMode.Pivot, evt.newValue));
            Add(snapPivotToggle);

            snapEdgesToggle = new Toggle("Snap Edges");
            snapEdgesToggle.style.marginBottom = 2f;
            snapEdgesToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetSnapModeEnabled(UIGuideSnapMode.Edges, evt.newValue));
            Add(snapEdgesToggle);

            snapIntersectionsToggle = new Toggle("Use Intersections");
            snapIntersectionsToggle.style.marginBottom = 6f;
            snapIntersectionsToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetSnapToIntersections(evt.newValue));
            Add(snapIntersectionsToggle);

            shortcutsLabel = new Label("Shortcuts: Alt+Shift+H / V / G / Backspace\nHold Ctrl/Cmd while dragging to bypass snap");
            shortcutsLabel.style.fontSize = 10f;
            shortcutsLabel.style.whiteSpace = WhiteSpace.Normal;
            shortcutsLabel.style.color = new StyleColor(new Color(0.68f, 0.72f, 0.78f));
            shortcutsLabel.style.marginBottom = 6f;
            Add(shortcutsLabel);

            VisualElement addRow = new();
            addRow.style.flexDirection = FlexDirection.Row;
            addRow.style.marginBottom = 6f;

            addHorizontalButton = new Button(() => UIGuidesController.AddGuide(UIGuideOrientation.Horizontal))
            {
                text = "+ H"
            };
            addHorizontalButton.style.flexGrow = 1f;
            addHorizontalButton.style.marginRight = 3f;
            addRow.Add(addHorizontalButton);

            addVerticalButton = new Button(() => UIGuidesController.AddGuide(UIGuideOrientation.Vertical))
            {
                text = "+ V"
            };
            addVerticalButton.style.flexGrow = 1f;
            addVerticalButton.style.marginLeft = 3f;
            addRow.Add(addVerticalButton);
            Add(addRow);

            selectionLabel = new Label();
            selectionLabel.style.marginTop = 4f;
            selectionLabel.style.marginBottom = 4f;
            selectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(selectionLabel);

            selectedPositionField = new FloatField("Position (px)");
            selectedPositionField.style.marginBottom = 4f;
            selectedPositionField.RegisterValueChangedCallback(evt => UIGuidesController.SetSelectedGuidePosition(evt.newValue));
            Add(selectedPositionField);

            selectedVisibleToggle = new Toggle("Selected Visible");
            selectedVisibleToggle.style.marginBottom = 4f;
            selectedVisibleToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetSelectedGuideVisible(evt.newValue));
            Add(selectedVisibleToggle);

            selectedLockedToggle = new Toggle("Selected Locked");
            selectedLockedToggle.style.marginBottom = 4f;
            selectedLockedToggle.RegisterValueChangedCallback(evt => UIGuidesController.SetSelectedGuideLocked(evt.newValue));
            Add(selectedLockedToggle);

            selectedColorField = new ColorField("Selected Color");
            selectedColorField.showAlpha = true;
            selectedColorField.style.marginBottom = 6f;
            selectedColorField.RegisterValueChangedCallback(evt => UIGuidesController.SetSelectedGuideColor(evt.newValue));
            Add(selectedColorField);

            VisualElement removeRow = new();
            removeRow.style.flexDirection = FlexDirection.Row;

            deleteButton = new Button(UIGuidesController.DeleteSelectedGuide)
            {
                text = "Delete"
            };
            deleteButton.style.flexGrow = 1f;
            deleteButton.style.marginRight = 3f;
            removeRow.Add(deleteButton);

            clearButton = new Button(UIGuidesController.ClearActiveCanvasGuides)
            {
                text = "Reset Canvas"
            };
            clearButton.style.flexGrow = 1f;
            clearButton.style.marginLeft = 3f;
            removeRow.Add(clearButton);
            Add(removeRow);

            schedule.Execute(Refresh).Every(150);
            Refresh();
        }

        private static bool IsFieldFocused(VisualElement field)
        {
            if (field?.focusController == null)
            {
                return false;
            }

            Focusable focused = field.focusController.focusedElement;
            if (focused is VisualElement element)
            {
                return element == field || (element.parent != null && element.parent == field);
            }

            return false;
        }

        private void Refresh()
        {
            Canvas activeCanvas = UIGuidesController.ActiveCanvas;
            bool hasCanvas = activeCanvas != null;

            titleLabel.text = hasCanvas ? $"Canvas: {activeCanvas.name}" : "Canvas: none";
            hintLabel.text = hasCanvas
                ? "Select a RectTransform and drag it near guides to snap."
                : "Select a Canvas or any child under one to start placing guides.";

            enabledToggle.SetValueWithoutNotify(UIGuidesController.IsEnabled);

            if (UIGuidesController.TryGetActiveCollection(out _, out UIGuideCollection collection))
            {
                visibleToggle.SetEnabled(true);
                defaultColorField.SetEnabled(true);
                coordinateOriginField.SetEnabled(true);
                snapEnabledToggle.SetEnabled(true);
                snapThresholdField.SetEnabled(true);
                snapPivotToggle.SetEnabled(true);
                snapEdgesToggle.SetEnabled(true);
                snapIntersectionsToggle.SetEnabled(true);
                visibleToggle.SetValueWithoutNotify(collection.settings.guidesVisible);
                defaultColorField.SetValueWithoutNotify(collection.settings.defaultColor);
                coordinateOriginField.SetValueWithoutNotify(UIGuidesUtility.GetCoordinateOriginLabel(collection.settings.coordinateOrigin));
                snapEnabledToggle.SetValueWithoutNotify(collection.settings.snapEnabled);
                snapThresholdField.SetValueWithoutNotify(collection.settings.snapThreshold);
                snapPivotToggle.SetValueWithoutNotify(collection.settings.snapMode.HasFlag(UIGuideSnapMode.Pivot));
                snapEdgesToggle.SetValueWithoutNotify(collection.settings.snapMode.HasFlag(UIGuideSnapMode.Edges));
                snapIntersectionsToggle.SetValueWithoutNotify(collection.settings.snapToIntersections);
                clearButton.SetEnabled(true);
            }
            else
            {
                visibleToggle.SetEnabled(hasCanvas);
                defaultColorField.SetEnabled(hasCanvas);
                coordinateOriginField.SetEnabled(hasCanvas);
                snapEnabledToggle.SetEnabled(hasCanvas);
                snapThresholdField.SetEnabled(hasCanvas);
                snapPivotToggle.SetEnabled(hasCanvas);
                snapEdgesToggle.SetEnabled(hasCanvas);
                snapIntersectionsToggle.SetEnabled(hasCanvas);
                visibleToggle.SetValueWithoutNotify(true);
                defaultColorField.SetValueWithoutNotify(new Color(1f, 0.35f, 0.1f, 0.95f));
                coordinateOriginField.SetValueWithoutNotify(UIGuidesUtility.GetCoordinateOriginLabel(UIGuideCoordinateOrigin.Center));
                snapEnabledToggle.SetValueWithoutNotify(false);
                snapThresholdField.SetValueWithoutNotify(8f);
                snapPivotToggle.SetValueWithoutNotify(true);
                snapEdgesToggle.SetValueWithoutNotify(true);
                snapIntersectionsToggle.SetValueWithoutNotify(true);
                clearButton.SetEnabled(false);
            }

            addHorizontalButton.SetEnabled(hasCanvas);
            addVerticalButton.SetEnabled(hasCanvas);
            visibleToggle.SetEnabled(hasCanvas);

            if (UIGuidesController.TryGetSelectedGuide(out Canvas selectedCanvas, out UIGuide selectedGuide))
            {
                UIGuideCoordinateOrigin origin = UIGuideCoordinateOrigin.Center;
                float displayPosition = selectedGuide.position;
                if (UIGuidesPersistenceService.TryGetCollection(selectedCanvas, out UIGuideCollection selectedCollection))
                {
                    origin = selectedCollection.settings.coordinateOrigin;
                }

                if (UIGuidesUtility.TryGetCanvasRect(selectedCanvas, out RectTransform canvasRect))
                {
                    displayPosition = UIGuidesUtility.ToDisplayGuidePosition(canvasRect, selectedGuide.orientation, selectedGuide.position, origin);
                }

                selectionLabel.text = $"Selected: {selectedGuide.orientation} @ {displayPosition:0.#} px ({UIGuidesUtility.GetCoordinateOriginLabel(origin)})";
                selectedPositionField.label = selectedGuide.orientation == UIGuideOrientation.Horizontal ? "Y (px)" : "X (px)";
                selectedPositionField.SetEnabled(!selectedGuide.locked);
                selectedVisibleToggle.SetEnabled(true);
                selectedLockedToggle.SetEnabled(true);
                selectedColorField.SetEnabled(true);
                deleteButton.SetEnabled(true);
                if (!IsFieldFocused(selectedPositionField))
                {
                    selectedPositionField.SetValueWithoutNotify(displayPosition);
                }
                selectedVisibleToggle.SetValueWithoutNotify(selectedGuide.visible);
                selectedLockedToggle.SetValueWithoutNotify(selectedGuide.locked);
                selectedColorField.SetValueWithoutNotify(selectedGuide.color);
            }
            else
            {
                selectionLabel.text = "Selected: none";
                selectedPositionField.label = "Position (px)";
                selectedPositionField.SetEnabled(false);
                selectedVisibleToggle.SetEnabled(false);
                selectedLockedToggle.SetEnabled(false);
                selectedColorField.SetEnabled(false);
                deleteButton.SetEnabled(false);
                selectedPositionField.SetValueWithoutNotify(0f);
                selectedVisibleToggle.SetValueWithoutNotify(true);
                selectedLockedToggle.SetValueWithoutNotify(false);
                selectedColorField.SetValueWithoutNotify(Color.white);
            }
        }

        private static UIGuideCoordinateOrigin ToCoordinateOrigin(string label)
        {
            return label switch
            {
                "Top Left" => UIGuideCoordinateOrigin.TopLeft,
                "Top Right" => UIGuideCoordinateOrigin.TopRight,
                "Bottom Left" => UIGuideCoordinateOrigin.BottomLeft,
                "Bottom Right" => UIGuideCoordinateOrigin.BottomRight,
                _ => UIGuideCoordinateOrigin.Center
            };
        }
    }
}
