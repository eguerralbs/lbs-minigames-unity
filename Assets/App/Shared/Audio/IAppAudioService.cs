using UnityEngine;

namespace Lbs.MiniGames.Shared.Audio
{
    /// <summary>
    /// Application-global audio contract. Music persists across scenes; Voice ducks music and interrupts itself; voice one-shots overlap; SFX is fire-and-forget.
    /// No singleton — instantiated and owned by ApplicationBootstrap, injected via AppServices.
    /// </summary>
    public interface IAppAudioService
    {
        void PlayMusic(AudioClip clip, bool loop = true, float volume = 0.25f);
        void StopMusic();
        bool IsMusicPlaying(AudioClip clip);
        void PlayVoice(AudioClip clip, float volume = 1f);
        void PlayVoiceOneShot(AudioClip clip, float volume = 1f);
        void StopVoice();
        void StopVoiceIfPlaying(AudioClip clip);
        bool IsVoicePlaying(AudioClip clip = null);
        void PlaySfx(AudioClip clip, float volumeScale = 1f);
        void PauseAll();
        void ResumeAll();
        void StopAll();
        void SetPaused(bool paused);
    }
}
