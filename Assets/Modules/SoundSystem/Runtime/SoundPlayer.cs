using UnityEngine;

namespace Modules.SoundSystems
{
    public class SoundPlayer : MonoBehaviour
    {
        // Clip Settings
        [SerializeField] private bool useDatabase;
        [SerializeField] private string audioKey;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private Audio.AudioType audioType;

        // Player Settings
        [SerializeField] private bool autoplay;
        [SerializeField] private bool is3DAudio;
        [SerializeField] private bool isLooped;

        public bool UseDatabase { get => useDatabase; set => useDatabase = value; }

        private void Start()
        {
            if (useDatabase)
            {
                if (SoundSystem.Instance.Database.TryGet(audioKey, out SoundDatabase.ItemPair result))
                {
                    audioClip = result.Value;
                    audioType = result.Type;
                }
            }

            if (autoplay)
            {
                Play();
            }
        }

        public void Play()
        {
            if (audioClip == null)
                return;

            switch (audioType)
            {
                case Audio.AudioType.Music:
                    SoundSystem.Instance.PlayMusic(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
                case Audio.AudioType.Sound:
                    SoundSystem.Instance.PlaySound(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
                case Audio.AudioType.UISound:
                    SoundSystem.Instance.PlayUISound(audioClip);
                    break;
                case Audio.AudioType.Ambience:
                    SoundSystem.Instance.PlayAmbience(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
                case Audio.AudioType.Voice:
                    SoundSystem.Instance.PlayVoice(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
            }
        }
    }
}