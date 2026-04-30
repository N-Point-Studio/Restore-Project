using System;
using System.Collections.Generic;
using UnityEngine;

namespace DMTools.UIGuides.Editor
{
    internal enum UIGuideOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    internal enum UIGuideCoordinateOrigin
    {
        Center = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4
    }

    [Flags]
    internal enum UIGuideSnapMode
    {
        None = 0,
        Pivot = 1 << 0,
        Edges = 1 << 1,
        Anchors = 1 << 2
    }

    [Serializable]
    internal sealed class UIGuide
    {
        public string id = Guid.NewGuid().ToString("N");
        public UIGuideOrientation orientation;
        public float position;
        public Color color = new(1f, 0.35f, 0.1f, 0.95f);
        public bool visible = true;
        public bool locked;

        public void Validate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
        }
    }

    [Serializable]
    internal sealed class UIGuideCanvasReference
    {
        public string canvasGlobalId;
        public string scenePath;
        public string canvasName;
        public string canvasHierarchyPath;

        public bool IsValid => !string.IsNullOrEmpty(canvasGlobalId) && !string.IsNullOrEmpty(scenePath);
    }

    [Serializable]
    internal sealed class UIGuideCollectionSettings
    {
        public bool guidesVisible = true;
        public bool snapEnabled;
        public UIGuideSnapMode snapMode = UIGuideSnapMode.Pivot | UIGuideSnapMode.Edges;
        public bool snapToIntersections = true;
        public float snapThreshold = 8f;
        public Color defaultColor = new(1f, 0.35f, 0.1f, 0.95f);
        public UIGuideCoordinateOrigin coordinateOrigin = UIGuideCoordinateOrigin.Center;

        public void Validate()
        {
            snapThreshold = Mathf.Max(0f, snapThreshold);
            if (!Enum.IsDefined(typeof(UIGuideCoordinateOrigin), coordinateOrigin))
            {
                coordinateOrigin = UIGuideCoordinateOrigin.Center;
            }
        }
    }

    [Serializable]
    internal sealed class UIGuideCollection
    {
        public UIGuideCanvasReference canvas = new();
        public UIGuideCollectionSettings settings = new();
        public List<UIGuide> guides = new();

        public void Validate()
        {
            canvas ??= new UIGuideCanvasReference();
            settings ??= new UIGuideCollectionSettings();
            settings.Validate();

            guides ??= new List<UIGuide>();
            for (int i = guides.Count - 1; i >= 0; i--)
            {
                UIGuide guide = guides[i];
                if (guide == null)
                {
                    guides.RemoveAt(i);
                    continue;
                }

                guide.Validate();
            }
        }
    }
}
