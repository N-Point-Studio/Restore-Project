using System;
using System.Collections.Generic;
using UnityEngine;

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
        }
    }
}