using UnityEngine;

namespace Lbs.MiniGames.Shared.Audio
{
    /// <summary>
    /// Immutable global audio configuration. Authored as ScriptableObject, never mutated at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "LBS Mini Games/Audio/App Audio Config", fileName = "AppAudioConfig")]
    public sealed class AppAudioConfig : ScriptableObject
    {
        [SerializeField] private AudioClip globalMusic;
        [SerializeField] private AudioClip hubMusic;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;
        [SerializeField, Range(0f, 1f)] private float duckedMusicVolume = 0.125f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        public AudioClip GlobalMusic => globalMusic;
        public AudioClip HubMusic => hubMusic != null ? hubMusic : globalMusic;
        public float MusicVolume => musicVolume;
        public float DuckedMusicVolume => duckedMusicVolume;
        public float VoiceVolume => voiceVolume;
        public float SfxVolume => sfxVolume;

        public bool IsValid()
        {
            return musicVolume >= 0f && musicVolume <= 1f
                   && duckedMusicVolume >= 0f && duckedMusicVolume <= 1f
                   && voiceVolume >= 0f && voiceVolume <= 1f
                   && sfxVolume >= 0f && sfxVolume <= 1f;
        }

#if UNITY_EDITOR
        public void Configure(AudioClip music, float musicVol = 0.25f, float duckedVol = 0.125f)
        {
            globalMusic = music;
            musicVolume = Mathf.Clamp01(musicVol);
            duckedMusicVolume = Mathf.Clamp01(duckedVol);
        }
#endif

        /// <summary>
        /// Runtime-safe factory for transient fallback config. Does not mutate persisted assets.
        /// </summary>
        public static AppAudioConfig CreateRuntimeFallback(AudioClip clip, float musicVol = 0.25f, float duckedVol = 0.125f)
        {
            var config = CreateInstance<AppAudioConfig>();
            config.globalMusic = clip;
            config.musicVolume = Mathf.Clamp01(musicVol);
            config.duckedMusicVolume = Mathf.Clamp01(duckedVol);
            config.voiceVolume = 1f;
            config.sfxVolume = 1f;
            return config;
        }
    }
}
