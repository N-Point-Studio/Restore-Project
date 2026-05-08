using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DMTools.UIGuides.Editor
{
    internal static class UIGuidesPersistenceService
    {
        private static UIGuidesDatabase Database => UIGuidesDatabase.instance;

        public static bool ToolEnabled
        {
            get => Database.ToolEnabled;
            set
            {
                if (Database.ToolEnabled == value)
                {
                    return;
                }

                Database.RecordChange("Toggle UI Guides");
                Database.ToolEnabled = value;
                Database.SaveData();
            }
        }

        public static bool TryGetCollection(Canvas canvas, out UIGuideCollection collection)
        {
            collection = null;
            if (!TryGetCanvasGlobalId(canvas, out string canvasGlobalId))
            {
                return false;
            }

            Database.ValidateData();
            if (!Database.TryGetCollectionByCanvasId(canvasGlobalId, out collection))
            {
                return false;
            }

            bool changed = SyncCanvasReference(collection, canvas);
            collection.Validate();
            if (changed)
            {
                Database.SaveData();
            }

            return true;
        }

        public static UIGuideCollection GetOrCreateCollection(Canvas canvas, bool recordUndo = false, string undoName = "Create UI Guide Collection")
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            if (!TryGetCanvasGlobalId(canvas, out string canvasGlobalId))
            {
                throw new InvalidOperationException("Canvas must have a valid GlobalObjectId before guide data can be stored.");
            }

            Database.ValidateData();
            if (Database.TryGetCollectionByCanvasId(canvasGlobalId, out UIGuideCollection existingCollection))
            {
                if (SyncCanvasReference(existingCollection, canvas))
                {
                    Database.SaveData();
                }

                return existingCollection;
            }

            if (recordUndo)
            {
                Database.RecordChange(undoName);
            }

            UIGuideCollection collection = new();
            PopulateCanvasReference(collection, canvas);
            collection.Validate();
            Database.Collections.Add(collection);
            Database.SaveData();
            return collection;
        }

        public static UIGuide AddGuide(Canvas canvas, UIGuideOrientation orientation, float position)
        {
            Database.RecordChange("Add UI Guide");
            UIGuideCollection collection = GetOrCreateCollection(canvas);
            UIGuide guide = new()
            {
                orientation = orientation,
                position = position,
                color = collection.settings.defaultColor
            };

            guide.Validate();
            collection.guides.Add(guide);
            collection.Validate();
            Database.SaveData();
            return guide;
        }

        public static void DeleteGuide(Canvas canvas, string guideId)
        {
            if (!TryGetCollection(canvas, out UIGuideCollection collection))
            {
                return;
            }

            UIGuide guide = collection.guides.FirstOrDefault(item => item.id == guideId);
            if (guide == null)
            {
                return;
            }

            Database.RecordChange("Delete UI Guide");
            collection.guides.Remove(guide);
            Database.SaveData();
        }

        public static void UpdateGuide(Canvas canvas, string guideId, Action<UIGuide> updater, string undoName)
        {
            if (!TryGetCollection(canvas, out UIGuideCollection collection))
            {
                return;
            }

            UIGuide guide = collection.guides.FirstOrDefault(item => item.id == guideId);
            if (guide == null)
            {
                return;
            }

            Database.RecordChange(undoName);
            updater?.Invoke(guide);
            guide.Validate();
            Database.SaveData();
        }

        public static void UpdateCollection(Canvas canvas, Action<UIGuideCollection> updater, string undoName)
        {
            if (canvas == null)
            {
                return;
            }

            Database.RecordChange(undoName);
            UIGuideCollection collection = GetOrCreateCollection(canvas);
            updater?.Invoke(collection);
            collection.Validate();
            SyncCanvasReference(collection, canvas);
            Database.SaveData();
        }

        public static bool BeginMoveGuide(Canvas canvas, string guideId)
        {
            if (!TryGetCollection(canvas, out UIGuideCollection collection))
            {
                return false;
            }

            return collection.guides.Any(item => item.id == guideId) && BeginTransientChange("Move UI Guide");
        }

        public static void SetGuidePositionWithoutUndo(Canvas canvas, string guideId, float position)
        {
            if (!TryGetCollection(canvas, out UIGuideCollection collection))
            {
                return;
            }

            UIGuide guide = collection.guides.FirstOrDefault(item => item.id == guideId);
            if (guide == null || Mathf.Approximately(guide.position, position))
            {
                return;
            }

            guide.position = position;
            Database.MarkDirty();
        }

        public static void SavePendingChanges()
        {
            Database.SaveData();
        }

        public static void ResetCanvasData(Canvas canvas)
        {
            if (!TryGetCanvasGlobalId(canvas, out string canvasGlobalId))
            {
                return;
            }

            Database.ValidateData();
            if (!Database.TryGetCollectionByCanvasId(canvasGlobalId, out UIGuideCollection collection))
            {
                return;
            }

            Database.RecordChange("Reset UI Guide Data");
            Database.Collections.Remove(collection);
            Database.SaveData();
        }

        public static void RemoveMissingCollections()
        {
            Database.ValidateData();
            bool removedAny = false;

            for (int i = Database.Collections.Count - 1; i >= 0; i--)
            {
                UIGuideCollection collection = Database.Collections[i];
                if (collection == null || collection.canvas == null || !collection.canvas.IsValid)
                {
                    Database.Collections.RemoveAt(i);
                    removedAny = true;
                    continue;
                }

                if (!GlobalObjectId.TryParse(collection.canvas.canvasGlobalId, out GlobalObjectId globalId))
                {
                    Database.Collections.RemoveAt(i);
                    removedAny = true;
                    continue;
                }

                if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) is Canvas canvas)
                {
                    if (SyncCanvasReference(collection, canvas))
                    {
                        removedAny = true;
                    }

                    continue;
                }

                Database.Collections.RemoveAt(i);
                removedAny = true;
            }

            if (removedAny)
            {
                Database.SaveData();
            }
        }

        private static bool BeginTransientChange(string undoName)
        {
            Database.RecordChange(undoName);
            return true;
        }

        private static bool TryGetCanvasGlobalId(Canvas canvas, out string canvasGlobalId)
        {
            canvasGlobalId = null;
            if (canvas == null)
            {
                return false;
            }

            canvasGlobalId = UIGuidesUtility.GetCanvasGlobalId(canvas);
            return !string.IsNullOrEmpty(canvasGlobalId);
        }

        private static bool SyncCanvasReference(UIGuideCollection collection, Canvas canvas)
        {
            if (collection == null || canvas == null)
            {
                return false;
            }

            string scenePath = canvas.gameObject.scene.path;
            string canvasName = canvas.name;
            string hierarchyPath = UIGuidesUtility.GetHierarchyPath(canvas.transform);
            string canvasGlobalId = UIGuidesUtility.GetCanvasGlobalId(canvas);

            bool changed = collection.canvas.canvasGlobalId != canvasGlobalId ||
                collection.canvas.scenePath != scenePath ||
                collection.canvas.canvasName != canvasName ||
                collection.canvas.canvasHierarchyPath != hierarchyPath;

            if (!changed)
            {
                return false;
            }

            PopulateCanvasReference(collection, canvas);
            return true;
        }

        private static void PopulateCanvasReference(UIGuideCollection collection, Canvas canvas)
        {
            collection.canvas.canvasGlobalId = UIGuidesUtility.GetCanvasGlobalId(canvas);
            collection.canvas.scenePath = canvas.gameObject.scene.path;
            collection.canvas.canvasName = canvas.name;
            collection.canvas.canvasHierarchyPath = UIGuidesUtility.GetHierarchyPath(canvas.transform);
        }
    }
}



