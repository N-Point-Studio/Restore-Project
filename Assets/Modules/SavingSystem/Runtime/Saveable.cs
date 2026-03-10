using SimpleJSON;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.SavingSystems
{
    [ExecuteAlways, DisallowMultipleComponent, DefaultExecutionOrder(-1)]
    public class Saveable : MonoBehaviour
    {
        [SerializeField] private string uid;
        public string UID => uid;

        private SavingSystem savingSystem;

        private static Dictionary<string, Saveable> globalLookup = new Dictionary<string, Saveable>();

        [Inject]
        public void Construct(SavingSystem savingSystem)
        {
            this.savingSystem = savingSystem;
            savingSystem.Register(this);
        }

        public JSONNode AsJSON()
        {
            JSONObject obj = new JSONObject();
            ISaveable[] saveables = GetComponents<ISaveable>();

            for (int i = 0; i < saveables.Length; i++)
            {
                obj[saveables[i].GetType().ToString()] = saveables[i].AsJSON();
            }

            return obj;
        }

        public async void LoadFromJSON(JSONNode obj)
        {
            ISaveable[] saveables = GetComponents<ISaveable>();
            for (int i = 0; i < saveables.Length; i++)
            {
                if (obj.HasKey(saveables[i].GetType().ToString()))
                {
                    saveables[i].LoadFromJSON(obj[saveables[i].GetType().ToString()].AsObject);
                }
                else
                {
                    saveables[i].LoadFromJSON("{}");
                }
            }

            await Task.CompletedTask;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.IsPlaying(gameObject)) return;
            if (string.IsNullOrEmpty(gameObject.scene.path)) return;

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty property = serializedObject.FindProperty("uid");

            if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            }

            if (globalLookup != null)
            {
                globalLookup[property.stringValue] = this;
            }
#endif
        }

        private bool IsUnique(string candidate)
        {
            if (globalLookup == null) return true;

            if (!globalLookup.ContainsKey(candidate)) return true;

            if (globalLookup[candidate] == this) return true;

            if (globalLookup[candidate] == null)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            if (globalLookup[candidate].uid != candidate)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            return false;
        }

        private void OnValidate()
        {
#if UNITY_6000_0_OR_NEWER
            LifetimeScope lifetimeScope = FindFirstObjectByType<LifetimeScope>();
#else
            LifetimeScope lifetimeScope = FindObjectOfType<LifetimeScope>();
#endif

            if (lifetimeScope == null)
                throw new System.NullReferenceException($"Could not find object with type {nameof(LifetimeScope)}");

            if (lifetimeScope.autoInjectGameObjects.Contains(gameObject)) return;

            lifetimeScope.autoInjectGameObjects.Add(gameObject);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying) return;

            savingSystem.Unregister(this);
        }
    }
}