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
        protected readonly List<Saveable> entities = new List<Saveable>();
        protected string rootPath;

        public SavingSystem()
        {
            rootPath = Path.Combine(Application.persistentDataPath, rootFolder);
        }

        public void Register(Saveable entity)
        {
            if (!entities.Contains(entity))
            {
                entities.Add(entity);
            }
        }

        public void Unregister(Saveable entity)
        {
            if (entities.Contains(entity))
            {
                entities.Remove(entity);
            }
        }

        public void ResetData()
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, true);
            }
        }

        public async void LoadEntities(System.Action onFinished = null)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                await LoadEntity(entities[i]);
            }

            onFinished?.Invoke();
        }

        public async Task LoadEntity(Saveable entity)
        {
            await RestoreFromToken(entity, LoadFromFile(entity.UID));
        }

        public void LoadFromFile(string saveFile, System.Action<JSONNode> onFinished)
        {
            onFinished?.Invoke(LoadFromFile(saveFile));
        }

        protected async Task RestoreFromToken(Saveable entity, JSONNode obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                entity.LoadFromJSON(obj);
            }
            else
            {
                entity.LoadFromJSON("{}");
            }

            await Task.CompletedTask;
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

        public async void SaveEntities(System.Action onFinished = null)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                await SaveFileAsJSON(entities[i].UID, entities[i].AsJSON());
            }

            onFinished?.Invoke();
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