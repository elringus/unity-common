using System;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityCommon
{
    /// <summary>
    /// Implementation is able to represent a playable audio track.
    /// </summary>
    public interface IAudioTrack
    {
        /// <summary>
        /// Invoked when the track has started playing.
        /// </summary>
        event Action OnPlay;
        /// <summary>
        /// Invoked when the track has finished playing or was stopped.
        /// </summary>
        event Action OnStop;

        /// <summary>
        /// Whether the track is currently playing.
        /// </summary>
        bool Playing { get; }
        /// <summary>
        /// Whether the track is looped (starts playing from start when finished).
        /// </summary>
        bool Loop { get; set; }
        /// <summary>
        /// Whether the track is muted (audio output is disabled, no matter <see cref="Volume"/> value).
        /// </summary>
        bool Mute { get; set; }
        /// <summary>
        /// Current volume of the track, in 0.0 to 1.0 range.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Starts playing the track.
        /// </summary>
        void Play ();
        /// <summary>
        /// Stops playing the track.
        /// </summary>
        void Stop ();
        /// <summary>
        /// Fades <see cref="Volume"/> to the provided value over the specified time, in seconds.
        /// </summary>
        UniTask FadeAsync (float volume, float fadeTime, AsyncToken asyncToken = default);
    }

    /// <summary>
    /// Represents and audio source with attached clip.
    /// </summary>
    public class AudioTrack : IAudioTrack
    {
        public event Action OnPlay;
        public event Action OnStop;

        public AudioClip Clip { get; }
        public AudioClip IntroClip { get; }
        public AudioSource Source { get; }
        public bool Valid => Clip && Source;
        public bool Loop { get => Valid && Source.loop; set { if (Valid) Source.loop = value; } }
        public bool Playing => Valid && Source.isPlaying;
        public bool Mute { get => Valid && Source.mute; set { if (Valid) Source.mute = value; } }
        public float Volume { get => Valid ? Source.volume : 0f; set { if (Valid) Source.volume = value; } }

        private readonly Tweener<FloatTween> volumeTweener;
        private readonly Timer stopTimer;

        public AudioTrack (AudioClip clip, AudioSource source, float volume = 1f, bool loop = false,
            AudioMixerGroup mixerGroup = null, AudioClip introClip = null)
        {
            Clip = clip;
            IntroClip = introClip;
            Source = source;
            Source.clip = Clip;
            Source.volume = volume;
            Source.loop = loop;
            Source.outputAudioMixerGroup = mixerGroup;

            volumeTweener = new Tweener<FloatTween>();
            stopTimer = new Timer(onCompleted: InvokeOnStop);
        }

        public void Play ()
        {
            CompleteAllRunners();
            if (!Valid) return;

            if (ObjectUtils.IsValid(IntroClip))
            {
                Source.PlayOneShot(IntroClip);
                Source.PlayScheduled(AudioSettings.dspTime + IntroClip.length);
                if (!Loop) stopTimer.Run(IntroClip.length + Clip.length, target: Source);
            }
            else
            {
                Source.Play();
                if (!Loop) stopTimer.Run(Clip.length, target: Source);
            }

            OnPlay?.Invoke();
        }

        public async UniTask PlayAsync (float fadeInTime, AsyncToken asyncToken = default)
        {
            CompleteAllRunners();
            if (!Valid) return;

            if (!Playing) Play();
            var tween = new FloatTween(0, Volume, fadeInTime, volume => Volume = volume, target: Source);
            await volumeTweener.RunAsync(tween, asyncToken);
        }

        public void Stop ()
        {
            CompleteAllRunners();
            if (!Valid) return;

            Source.Stop();

            OnStop?.Invoke();
        }

        public async UniTask StopAsync (float fadeOutTime, AsyncToken asyncToken = default)
        {
            CompleteAllRunners();
            if (!Valid) return;

            var tween = new FloatTween(Volume, 0, fadeOutTime, volume => Volume = volume, target: Source);
            await volumeTweener.RunAsync(tween, asyncToken);
            if (asyncToken.Canceled) return;
            Stop();
        }

        public async UniTask FadeAsync (float volume, float fadeTime, AsyncToken asyncToken = default)
        {
            CompleteAllRunners();
            if (!Valid) return;

            var tween = new FloatTween(Volume, volume, fadeTime, v => Volume = v, target: Source);
            await volumeTweener.RunAsync(tween, asyncToken);
        }

        private void CompleteAllRunners ()
        {
            if (volumeTweener.Running)
                volumeTweener.CompleteInstantly();
            if (stopTimer.Running)
                stopTimer.CompleteInstantly();
        }

        private void InvokeOnStop () => OnStop?.Invoke();
    }
}
