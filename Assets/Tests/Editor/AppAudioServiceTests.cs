using NUnit.Framework;
using UnityEngine;
using Lbs.MiniGames.Shared.Audio;

namespace Lbs.MiniGames.Tests
{
    public sealed class AppAudioServiceTests
    {
        private GameObject host;
        private AppAudioService service;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("AppAudioHost");
            service = host.AddComponent<AppAudioService>();
            service.Initialize(null);
        }

        [TearDown]
        public void TearDown()
        {
            if (service != null) service.StopAll();
            if (host != null) Object.DestroyImmediate(host);
        }

        [Test]
        public void IsVoicePlaying_ReturnsFalseInitially_AndHandlesNullClip()
        {
            Assert.IsFalse(service.IsVoicePlaying());
            Assert.IsFalse(service.IsVoicePlaying(null));
            var clip = AudioClip.Create("dummy", 441, 1, 44100, false);
            try
            {
                Assert.IsFalse(service.IsVoicePlaying(clip));
                Assert.IsFalse(service.IsMusicPlaying(clip));
            }
            finally { Object.DestroyImmediate(clip); }
        }

        [Test]
        public void PlayVoice_Null_DoesNotThrow_And_IsVoicePlayingStaysFalse()
        {
            Assert.DoesNotThrow(() => service.PlayVoice(null));
            Assert.IsFalse(service.IsVoicePlaying());
        }

        [Test]
        public void PlayVoiceOneShot_Null_DoesNotThrow_And_IsVoicePlayingStaysFalse()
        {
            Assert.DoesNotThrow(() => service.PlayVoiceOneShot(null));
            Assert.IsFalse(service.IsVoicePlaying());
        }

        [Test]
        public void PlayVoice_And_StopVoice_DoNotThrow_And_IsVoicePlayingContractHolds()
        {
            var clip = AudioClip.Create("voice", 441, 1, 44100, false);
            try
            {
                Assert.DoesNotThrow(() => service.PlayVoice(clip));
                // In EditMode isPlaying is false, so IsVoicePlaying will be false - we only verify StopVoice clears without throw
                Assert.DoesNotThrow(() => service.StopVoice());
                Assert.IsFalse(service.IsVoicePlaying(clip));
                Assert.IsFalse(service.IsVoicePlaying());
            }
            finally { Object.DestroyImmediate(clip); }
        }

        [Test]
        public void DisablingHost_StopsAndClearsVoicePlayback()
        {
            var clip = AudioClip.Create("voice", 441, 1, 44100, false);
            try
            {
                service.PlayVoice(clip);
                AudioSource voiceSource = null;
                foreach (AudioSource source in host.GetComponents<AudioSource>())
                {
                    if (source.clip == clip)
                    {
                        voiceSource = source;
                        break;
                    }
                }

                Assert.IsNotNull(voiceSource);

                host.SetActive(false);

                Assert.IsNull(voiceSource.clip);
                Assert.IsFalse(voiceSource.isPlaying);
                Assert.IsFalse(service.IsVoicePlaying());
            }
            finally { Object.DestroyImmediate(clip); }
        }

        [Test]
        public void PlayMusic_Idempotent_SameClipDoesNotThrow()
        {
            var clip = AudioClip.Create("music", 441, 1, 44100, false);
            try
            {
                service.PlayMusic(clip, true, 0.25f);
                Assert.DoesNotThrow(() => service.PlayMusic(clip, true, 0.25f));
                Assert.DoesNotThrow(() => service.StopMusic());
            }
            finally { Object.DestroyImmediate(clip); }
        }
    }
}
