using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityCommon
{
    /// <summary>
    /// Represents and audio source with attached clip.
    /// </summary>
    public class AudioTrack
    {
        /// <summary>
        /// Invoked when the track has started playing.
        /// </summary>
        public event Action OnPlay;
        /// <summary>
        /// Invoked when the track has finished playing or was stopped.
        /// </summary>
        public event Action OnStop;

        public string Name => Clip.name;
        public AudioClip Clip { get; private set; }
        public AudioClip IntroClip { get; private set; }
        public AudioSource Source { get; private set; }
        public bool IsValid => Clip && Source;
        public bool IsLooped { get => IsValid ? Source.loop : false; set { if (IsValid) Source.loop = value; } }
        public bool IsPlaying => IsValid ? Source.isPlaying : false;
        public bool IsMuted { get => IsValid ? Source.mute : false; set { if (IsValid) Source.mute = value; } }
        public float Volume { get => IsValid ? Source.volume : 0f; set { if (IsValid) Source.volume = value; } }

        private readonly Tweener<FloatTween> volumeTweener;
        private readonly Timer stopTimer;

        public AudioTrack (AudioClip clip, AudioSource source, MonoBehaviour behaviourContainer = null,
            float volume = 1f, bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null)
        {
            Clip = clip;
            IntroClip = introClip;
            Source = source;
            Source.clip = Clip;
            Source.volume = volume;
            Source.loop = loop;
            Source.outputAudioMixerGroup = mixerGroup;

            volumeTweener = new Tweener<FloatTween>(behaviourContainer);
            stopTimer = new Timer(coroutineContainer: behaviourContainer, onCompleted: InvokeOnStop);
        }

        public void Play ()
        {
            CompleteAllRunners();
            if (!IsValid) return;

            if (ObjectUtils.IsValid(IntroClip))
            {
                Source.PlayOneShot(IntroClip);
                Source.PlayScheduled(AudioSettings.dspTime + IntroClip.length);
                if (!IsLooped) stopTimer.Run(IntroClip.length + Clip.length);
            }
            else
            {
                Source.Play();
                if (!IsLooped) stopTimer.Run(Clip.length);
            }

            OnPlay?.Invoke();
        }

        public async Task PlayAsync (float fadeInTime, CancellationToken cancellationToken = default)
        {
            CompleteAllRunners();
            if (!IsValid) return;

            if (!IsPlaying) Play();
            var tween = new FloatTween(0, Volume, fadeInTime, volume => Volume = volume);
            await volumeTweener.RunAsync(tween, cancellationToken);
        }

        public void Stop ()
        {
            CompleteAllRunners();
            if (!IsValid) return;

            Source.Stop();

            OnStop?.Invoke();
        }

        public async Task StopAsync (float fadeOutTime, CancellationToken cancellationToken = default)
        {
            CompleteAllRunners();
            if (!IsValid) return;

            var tween = new FloatTween(Volume, 0, fadeOutTime, volume => Volume = volume);
            await volumeTweener.RunAsync(tween, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            Stop();
        }

        public async Task FadeAsync (float volume, float fadeTime, CancellationToken cancellationToken = default)
        {
            CompleteAllRunners();
            if (!IsValid) return;

            var tween = new FloatTween(Volume, volume, fadeTime, v => Volume = v);
            await volumeTweener.RunAsync(tween, cancellationToken);
        }

        private void CompleteAllRunners ()
        {
            if (volumeTweener.IsRunning)
                volumeTweener.CompleteInstantly();
            if (stopTimer.IsRunning)
                stopTimer.CompleteInstantly();
        }

        private void InvokeOnStop () => OnStop?.Invoke();
    }
}
