using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DMTools.UIGuides.Editor
{
    [FilePath("ProjectSettings/UIGuidesDatabase.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class UIGuidesDatabase : ScriptableSingleton<UIGuidesDatabase>
    {
        [UnityEngine.SerializeField] private bool toolEnabled = true;
        [UnityEngine.SerializeField] private List<UIGuideCollection> collections = new();

        public bool ToolEnabled
        {
            get => toolEnabled;
            set => toolEnabled = value;
        }

        public List<UIGuideCollection> Collections => collections;

        public bool TryGetCollectionByCanvasId(string canvasGlobalId, out UIGuideCollection collection)
        {
            collection = collections.FirstOrDefault(item => item != null && item.canvas != null && item.canvas.canvasGlobalId == canvasGlobalId);
            return collection != null;
        }

        public void ValidateData()
        {
            collections ??= new List<UIGuideCollection>();

            for (int i = collections.Count - 1; i >= 0; i--)
            {
                UIGuideCollection collection = collections[i];
                if (collection == null)
                {
                    collections.RemoveAt(i);
                    continue;
                }

                collection.Validate();
            }
        }

        public void RecordChange(string undoName)
        {
            Undo.RecordObject(this, undoName);
            EditorUtility.SetDirty(this);
        }

        public void MarkDirty()
        {
            EditorUtility.SetDirty(this);
        }

        public void SaveData()
        {
            MarkDirty();
            Save(true);
        }
    }
}



