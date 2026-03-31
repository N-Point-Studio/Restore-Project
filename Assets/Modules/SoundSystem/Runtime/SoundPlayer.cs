using UnityEngine;

namespace Modules.SoundSystems
{
    public class SoundPlayer : MonoBehaviour
    {
        // Clip Settings
        [SerializeField] private bool useDatabase;
        [SerializeField] private AudioKey audioKey;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private Audio.AudioType audioType;

        // Player Settings
        [SerializeField] private bool autoplay;
        [SerializeField] private bool is3DAudio;
        [SerializeField] private bool isLooped;

        [Header("Anti-Spam Settings")]
        [Tooltip("Aktifkan ini agar suara tidak menumpuk/spam saat dipanggil berkali-kali tiap frame")]
        [SerializeField] private bool preventSpam = true; 
        
        private int currentAudioId = -1;

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

            if (preventSpam)
            {
                Audio activeAudio = SoundSystem.Instance.GetAudio(currentAudioId);
                if (activeAudio != null && activeAudio.IsPlaying)
                {
                    return; 
                }
            }

            switch (audioType)
            {
                case Audio.AudioType.Music:
                    currentAudioId = SoundSystem.Instance.PlayMusic(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
                case Audio.AudioType.Sound:
                    currentAudioId = SoundSystem.Instance.PlaySound(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
                case Audio.AudioType.UISound:
                    currentAudioId = SoundSystem.Instance.PlayUISound(audioClip);
                    break;
                case Audio.AudioType.Ambience:
                    currentAudioId = SoundSystem.Instance.PlayAmbience(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
                case Audio.AudioType.Voice:
                    currentAudioId = SoundSystem.Instance.PlayVoice(audioClip, 1, isLooped, is3DAudio ? transform : null);
                    break;
            }
        }
    }
}