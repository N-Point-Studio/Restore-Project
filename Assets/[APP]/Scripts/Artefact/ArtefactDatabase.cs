using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Modules;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = nameof(ArtefactDatabase), menuName = "App/Database/Artefact Database")]
public class ArtefactDatabase : BaseDatabase<ArtefactData>
{
    [System.Serializable]
    public class ArtefactSpellPair : DatabaseItemPair<ArtefactData>
    {
        public ArtefactSpellPair(string key, ArtefactData value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Header("Artefacts")]
    [SerializeField] private List<ArtefactSpellPair> items = new List<ArtefactSpellPair>();

    private readonly Dictionary<string, ArtefactData> lookup = new Dictionary<string, ArtefactData>();

#if UNITY_EDITOR
    public override void Setup()
    {
        Clear();

        string[] guids = AssetDatabase.FindAssets("t:scriptableobject", searchFolders);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ArtefactData item = AssetDatabase.LoadAssetAtPath<ArtefactData>(path);

            if (item == null)
                continue;

            if (item.BaseData.Exclude)
                continue;

            Add(item);
        }

        Sort();
    }
#endif
    public override ArtefactData[] GetAllItems()
    {
        ArtefactData[] arr = new ArtefactData[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            arr[i] = items[i]?.Value;
        }
        return arr;
    }

    public override ArtefactData GetItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;
        return items[index]?.Value;
    }

    public override ArtefactData GetItem(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        EnsureIndex();
        if (lookup.TryGetValue(id, out var val))
            return val;

        return null;
    }

    public override ArtefactData GetRandom()
    {
        if (items == null || items.Count == 0)
            return null;
        int idx = UnityEngine.Random.Range(0, items.Count);
        return items[idx]?.Value;
    }

    /// <summary>
    /// Get an artefact by its ID.
    /// </summary>
    public ArtefactData GetArtefact(string artefactId)
    {
        return GetItem(artefactId);
    }

    /// <summary>
    /// Get all artefacts of a specific element type.
    /// </summary>
    public List<ArtefactData> GetArtefactsByAge(ArtefactAge artefactAge)
    {
        return GetAllItems().Where(a => a.ArtefactAge == artefactAge).ToList();
    }

    /// <summary>
    /// Get all artefacts that are unlocked by default (starters).
    /// </summary>
    public List<ArtefactData> GetDefaultUnlockedArtefacts()
    {
        return GetAllItems().Where(a => a.UnlockedByDefault).ToList();
    }

    /// <summary>
    /// Get all artefacts.
    /// </summary>
    public List<ArtefactData> GetAllArtefacts()
    {
        return GetAllItems().ToList();
    }

#if UNITY_EDITOR
    protected override void Add(ArtefactData value)
    {
        if (value == null)
            return;

        var key = GetKey(value);
        if (string.IsNullOrEmpty(key))
            return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].Key == key)
                return;
        }

        items.Add(new ArtefactSpellPair(key, value));
        if (!lookup.ContainsKey(key))
            lookup.Add(key, value);
    }

    protected override void Clear()
    {
        items.Clear();
        lookup.Clear();
    }

    protected override void Sort()
    {
        items.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return string.Compare(a.Key, b.Key, System.StringComparison.Ordinal);
        });

        RebuildIndex();
    }
#endif

    private static string GetKey(ArtefactData data)
    {
        return data != null ? data.BaseData.Id : null;
    }

    private void EnsureIndex()
    {
        if (lookup.Count == items.Count)
            return;
        RebuildIndex();
    }

    private void RebuildIndex()
    {
        lookup.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            var p = items[i];
            if (p == null || p.Value == null)
                continue;
            var key = p.Key;
            if (string.IsNullOrEmpty(key))
                key = GetKey(p.Value);
            if (string.IsNullOrEmpty(key))
                continue;
            if (!lookup.ContainsKey(key))
                lookup.Add(key, p.Value);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ArtefactDatabase))]
public class ArtefactDatabaseEditor : Editor
{
    private ArtefactDatabase script;

    private void OnEnable()
    {
        script = (ArtefactDatabase)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space();
        if (GUILayout.Button("Setup"))
        {
            script.Setup();
            EditorUtility.SetDirty(script);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
