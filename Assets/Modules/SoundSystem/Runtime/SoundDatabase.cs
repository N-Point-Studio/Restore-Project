using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;
#endif

namespace Modules.SoundSystems
{
    [CreateAssetMenu(fileName = nameof(SoundDatabase), menuName = "App/Sound System/Create Database")]
    public class SoundDatabase : ScriptableObject
    {
        [Header("Sounds")]
        [SerializeField] private List<ItemPairGroup> sounds;

        [Header("Settings")]
        [SerializeField] private bool randomizePitchForDuplicates;
        public bool RandomizePitchForDuplicates => randomizePitchForDuplicates;

        [SerializeField] private float minPitch;
        public float MinPitch => minPitch;

        [SerializeField] private float maxPitch;
        public float MaxPitch => maxPitch;

        public bool TryGet(AudioKey key, out ItemPair result)
        {
            return TryGet(key.ToString(), out result);
        }

        public ItemPair Get(AudioKey key)
        {
            return Get(key.ToString());
        }

        public bool TryGet(string key, out ItemPair result)
        {
            result = Get(key);
            return result != null;
        }

        public ItemPair Get(string key)
        {
            ItemPair item = null;
            for (int i = 0; i < sounds.Count; i++)
            {
                for (int j = 0; j < sounds[i].items.Count; j++)
                {
                    if (sounds[i].items[j].Key == key)
                    {
                        item = sounds[i].items[j];
                    }
                }
            }

            if (item != null)
                return item;

            throw new NullReferenceException($"No audioclip with key '{key}' is exist");
        }

        [Serializable]
        public class ItemPairGroup
        {
            public string id;
            public List<ItemPair> items;
        }

        [Serializable]
        public class ItemPair
        {
            [SerializeField] protected string key;
            public string Key => key;

            [SerializeField, SerializeReference] protected AudioClip value;
            public AudioClip Value => value;

            [SerializeField] protected Audio.AudioType type;
            public Audio.AudioType Type => type;

            public ItemPair(string key, AudioClip value)
            {
                this.key = key;
                this.value = value;
            }

            public int CompareTo(ItemPair other)
            {
                return this.key.CompareTo(other.key);
            }

#if UNITY_EDITOR
            public void SetKey(string newKey)
            {
                this.key = newKey;
            }
#endif
        }

#if UNITY_EDITOR
        private string FormatToEnumName(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string cleanString = Regex.Replace(input, @"[^a-zA-Z0-9]", " ");
            
            string[] words = cleanString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                string lower = word.ToLower();

                if (lower == "ui" || lower == "sfx" || lower == "bgm" || lower == "bg" || lower == "ost" || lower == "vo")
                {
                    words[i] = word.ToUpper();
                }
                else
                {
                    words[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower();
                }
            }

            string result = string.Join("_", words);

            if (result.Length > 0 && char.IsDigit(result[0]))
            {
                result = "_" + result;
            }

            return result;
        }

        public void GenerateAudioEnum()
        {
            string folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
            string filePath = Path.Combine(folderPath, "AudioKey.cs");

            List<string> generatedKeys = new List<string>();
            bool isModified = false; 

            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine("// AUTO-GENERATED FILE. DO NOT MODIFY MANUALLY.");
                streamWriter.WriteLine("namespace Modules.SoundSystems");
                streamWriter.WriteLine("{");
                streamWriter.WriteLine("    public enum AudioKey");
                streamWriter.WriteLine("    {");
                streamWriter.WriteLine("        None, // Default fallback");

                foreach (var group in sounds)
                {
                    foreach (var item in group.items)
                    {
                        if (string.IsNullOrEmpty(item.Key)) continue;

                        string safeKey = FormatToEnumName(item.Key);

                        if (item.Key != safeKey)
                        {
                            item.SetKey(safeKey);
                            isModified = true;
                        }

                        if (!generatedKeys.Contains(safeKey))
                        {
                            streamWriter.WriteLine($"        {safeKey},");
                            generatedKeys.Add(safeKey);
                        }
                    }
                }

                streamWriter.WriteLine("    }");
                streamWriter.WriteLine("}");
            }

            if (isModified)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
            Debug.Log($"<b>AudioKey</b> enum successfully generated at: {filePath}");
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SoundDatabase))]
    public class SoundDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(15);

            if (GUILayout.Button("Generate Audio Enum", GUILayout.Height(35)))
            {
                SoundDatabase database = (SoundDatabase)target;
                database.GenerateAudioEnum();
            }
        }
    }
#endif
}