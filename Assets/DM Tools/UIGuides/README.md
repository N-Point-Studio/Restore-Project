# UI Guides

UI Guides is an editor-only uGUI composition tool for placing Figma-style horizontal and vertical alignment guides directly over a `Canvas` in the Unity Scene view.

It is built for UI artists, designers, and developers who want layout guides inside Unity without adding runtime components, scene behaviours, or build-time dependencies.

## Requirements

- Unity 2021.3 or newer
- uGUI / Unity UI package
- Editor-only workflow; no runtime setup required

## Highlights

- Scene view overlay named `UI Guides`
- Add horizontal and vertical guides per Canvas
- Drag guides directly in Scene view
- Choose whether guide coordinates start from the center or any Canvas corner
- Edit selected guide positions by exact px value in the overlay or directly in the Scene view label
- Optional snapping for selected `RectTransform` elements near guides
- Pivot and edge snapping modes with adjustable threshold
- Optional guide intersection snapping and visual snap hints
- Toggle guide visibility without disabling the tool
- Customize a default color and per-guide colors
- Delete the selected guide or reset all guide data for the active Canvas
- Keyboard shortcuts for add, delete, and visibility toggle
- Undo and redo support for guide creation, drag moves, edits, deletion, reset, and snap settings
- Per-Canvas persistence stored in `ProjectSettings/UIGuidesDatabase.asset`
- Zero runtime scripts or runtime assembly footprint

## Folder Layout

```text
Assets/
  DM Tools/
    UIGuides/
      Editor/
        UIGuides.Editor.asmdef
        Scripts/
      README.md
```

## Quick Start

1. Open a scene with a `Canvas`.
2. Open the Scene view overlay menu.
3. Enable `UI Guides` if it is not already visible.
4. Select a `Canvas` or any child under that Canvas.
5. Use the `UI Guides` overlay to add horizontal or vertical guides.

No runtime component is added to your scene or build. All scripts are under an Editor-only assembly definition.

For manual installation outside the Asset Store, copy the `Assets/DM Tools/UIGuides` folder into your Unity project.

## Use

1. Select a `Canvas` or any child under that Canvas.
2. In the `UI Guides` Scene overlay, click `Add Horizontal` or `Add Vertical`.
3. Use `Coordinate Origin` to choose whether px values are measured from the center, top left, top right, bottom left, or bottom right.
4. Select a guide to edit its exact `X` or `Y` px value in the overlay, or type directly into the selected guide label in Scene view.
5. Select a `RectTransform` under that Canvas and drag it near the guides.
6. Turn on `Enable Snap` to allow snapping while dragging.
7. Adjust `Threshold`, `Snap Pivot`, `Snap Edges`, and `Use Intersections` to match the layout task.
8. Hold `Ctrl` on Windows or `Cmd` on macOS while dragging to temporarily bypass snapping.
9. Toggle `Show Guides` to hide or reveal all guides for the active Canvas.
10. Use `Reset Canvas` to clear the current Canvas guide collection and its persisted settings.

## Snapping

Snapping is editor-only and only affects the currently selected `RectTransform` while it is being dragged in Scene view.
Snap behavior is driven by the active Canvas guide collection:

- `Enable Snap` turns snapping on or off per Canvas.
- `Threshold` controls how close an element must be before a guide attracts it.
- `Snap Pivot` aligns the element pivot to nearby guides.
- `Snap Edges` aligns left, right, top, and bottom edges to nearby guides.
- `Use Intersections` allows simultaneous horizontal and vertical snaps when both are within range.
- Holding `Ctrl` or `Cmd` while dragging temporarily disables snapping.

## Persistence

Guide data is stored in the editor-only asset `ProjectSettings/UIGuidesDatabase.asset`.
Each collection is keyed by the Canvas `GlobalObjectId` and also stores the scene path, Canvas name, and hierarchy path for validation and easier debugging.
Persisted guide fields:

- orientation
- position
- color
- visibility
- locked state

Persisted collection settings:

- global visibility
- default guide color
- snap enabled
- snap mode
- snap threshold
- intersection snapping
- coordinate origin

## Shortcuts

- `Alt+Shift+H`: add a horizontal guide
- `Alt+Shift+V`: add a vertical guide
- `Alt+Shift+G`: toggle guide visibility
- `Alt+Shift+Backspace`: delete the selected guide
- `Ctrl` on Windows or `Cmd` on macOS while dragging: temporarily bypass snapping

## Limitations

- Designed for uGUI `Canvas` and `RectTransform` workflows.
- Does not replace a grid system or runtime layout system.
- Guide data is project/editor data and is not intended to ship in player builds.

## Changelog

- Phase 1: introduced the core Scene view overlay for adding, selecting, dragging, coloring, showing, hiding, locking, deleting, and resetting Canvas-based UI guides.
- Phase 2: polished Scene view UX with clearer selected and hover states, smoother drag behavior, Scene view shortcuts, and a more compact overlay.
- Phase 3: refactored persistence into explicit guide, collection settings, and canvas reference models with an editor-only persistence service, per-Canvas reset, and stale data validation.
- Phase 4: added optional RectTransform snapping with pivot and edge modes, adjustable thresholds, intersection support, temporary snap bypass, and visual snap hints in Scene view.
- Phase 5: added exact selected guide position editing from both the `UI Guides` overlay and the editable black Scene view label.
- Phase 6: added selectable coordinate origins for center, top-left, top-right, bottom-left, and bottom-right guide measurements.

## Notes

- The tool is designed for uGUI `Canvas` and `RectTransform` workflows.
- No runtime component is added to scene objects.
- The package intentionally avoids acting as a grid system replacement.
