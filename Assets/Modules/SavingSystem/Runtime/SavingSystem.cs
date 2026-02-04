using SimpleJSON;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.SavingSystems
{
    public class SavingSystem
    {
        protected const string rootFolder = "saves";
        protected const string extension = ".dat";
        protected string rootPath;

        public SavingSystem()
        {
            rootPath = Path.Combine(Application.persistentDataPath, rootFolder);
        }

        public void ResetData()
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, true);
            }
        }

        public void LoadFromFile(string saveFile, System.Action<JSONNode> onFinished)
        {
            onFinished?.Invoke(LoadFromFile(saveFile));
        }

        protected JSONNode LoadFromFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path))
            {
                return new JSONObject();
            }

            using (StreamReader reader = new StreamReader(path))
            {
                string data = reader.ReadToEnd();
                reader.Close();

                if (string.IsNullOrEmpty(data))
                {
                    return JSON.Parse("{}");
                }
                else
                {
                    return JSON.Parse(data);
                }
            }
        }

        public async void SaveToFile(string saveFile, JSONNode data, System.Action onFinished = null)
        {
            await SaveFileAsJSON(saveFile, data);
            onFinished?.Invoke();
        }

        protected async Task SaveFileAsJSON(string saveFile, JSONNode data)
        {
            string path = GetPathFromSaveFile(saveFile);

            if (!File.Exists(path))
            {
                new FileInfo(path).Directory.Create();
                FileStream file = File.Create(path);
                file.Close();
            }

            using (StreamWriter writer = File.CreateText(path))
            {
                writer.Write(data.ToString());
                writer.Flush();
            }

            await Task.CompletedTask;
        }

        protected string GetPathFromSaveFile(string saveFile)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                rootPath = Path.Combine(Application.persistentDataPath, rootFolder);
            }

            return Path.Combine(rootPath, saveFile + extension);
        }

#if UNITY_EDITOR
        [MenuItem("Tools/App/Open Save Folder")]
        private static void OpenSaveDataFolder()
        {
            EditorUtility.RevealInFinder(Path.Combine(Application.persistentDataPath, rootFolder));
        }
#endif
    }
}