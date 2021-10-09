using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityCommon
{
    /// <summary>
    /// Manages <see cref="AudioTrack"/> objects.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        public AudioListener Listener => listenerCache ? listenerCache : FindOrAddListener();
        public float Volume { get => AudioListener.volume; set => AudioListener.volume = value; }
        public bool Mute { get => AudioListener.pause; set => AudioListener.pause = value; }

        private readonly List<AudioTrack> audioTracks = new List<AudioTrack>();
        private readonly Stack<AudioSource> sourcesPool = new Stack<AudioSource>();
        private AudioListener listenerCache;
        private Tweener<FloatTween> listenerVolumeTweener;

        private void Awake ()
        {
            listenerVolumeTweener = new Tweener<FloatTween>();
            FindOrAddListener();
        }

        /// <summary>
        /// Sets transform of the current <see cref="Listener"/> as a child of the provided target.
        /// </summary>
        public void AttachListener (Transform target)
        {
            Listener.transform.SetParent(target);
            Listener.transform.localPosition = Vector3.zero;
        }

        public void FadeVolume (float volume, float time)
        {
            if (listenerVolumeTweener.Running)
                listenerVolumeTweener.CompleteInstantly();

            var tween = new FloatTween(Volume, volume, time, value => Volume = value, ignoreTimeScale: true);
            listenerVolumeTweener.Run(tween, target: this);
        }

        public bool ClipPlaying (AudioClip clip)
        {
            if (!clip) return false;
            return audioTracks.Any(t => t.Clip == clip && t.Playing);
        }

        public void PlayClip (AudioClip clip, AudioSource audioSource = null, float volume = 1f, 
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, bool additive = false)
        {
            if (!clip) return;

            if (!additive) StopClip(clip);
            PoolUnusedSources();

            // In case user somehow provided one of our pooled sources, don't use it.
            if (audioSource && IsOwnedByController(audioSource)) audioSource = null;
            if (!audioSource) audioSource = GetPooledSource();

            var track = new AudioTrack(clip, audioSource, volume, loop, mixerGroup, introClip);
            audioTracks.Add(track);
            track.Play();
        }

        public async UniTask PlayClipAsync (AudioClip clip, float fadeInTime, AudioSource audioSource = null, float volume = 1f,
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, bool additive = false, AsyncToken asyncToken = default)
        {
            if (!clip) return;

            if (!additive) StopClip(clip);
            PoolUnusedSources();

            // In case user somehow provided one of our pooled sources, don't use it.
            if (audioSource && IsOwnedByController(audioSource)) audioSource = null;
            if (!audioSource) audioSource = GetPooledSource();

            var track = new AudioTrack(clip, audioSource, volume, loop, mixerGroup, introClip);
            audioTracks.Add(track);
            await track.PlayAsync(fadeInTime, asyncToken);
        }

        public void StopClip (AudioClip clip)
        {
            if (!clip || !ClipPlaying(clip)) return;
            foreach (var track in GetTracks(clip))
                track.Stop();
        }

        public void StopAllClips ()
        {
            foreach (var track in GetAllTracks())
                track.Stop();
        }

        public async UniTask StopClipAsync (AudioClip clip, float fadeOutTime, AsyncToken asyncToken = default)
        {
            if (!clip || !ClipPlaying(clip)) return;
            var tasks = new List<UniTask>();
            foreach (var track in GetTracks(clip))
                tasks.Add(track.StopAsync(fadeOutTime, asyncToken));
            await UniTask.WhenAll(tasks);
        }

        public async UniTask StopAllClipsAsync (float fadeOutTime, AsyncToken asyncToken = default)
        {
            var tasks = new List<UniTask>();
            foreach (var track in GetAllTracks())
                tasks.Add(track.StopAsync(fadeOutTime, asyncToken));
            await UniTask.WhenAll(tasks);
        }

        public IReadOnlyCollection<AudioTrack> GetTracks (AudioClip clip)
        {
            if (!clip) return null;
            return audioTracks.Where(t => t.Clip == clip).ToArray();
        }
        
        public IReadOnlyCollection<AudioTrack> GetAllTracks ()
        {
            return audioTracks.ToArray();
        }

        private AudioListener FindOrAddListener ()
        {
            listenerCache = FindObjectOfType<AudioListener>();
            if (!listenerCache) listenerCache = gameObject.AddComponent<AudioListener>();
            return listenerCache;
        }

        private bool IsOwnedByController (AudioSource audioSource)
        {
            return audioSource && audioSource.gameObject == gameObject;
        }

        private AudioSource GetPooledSource ()
        {
            if (sourcesPool.Count > 0) return sourcesPool.Pop();
            return gameObject.AddComponent<AudioSource>();
        }

        private void PoolUnusedSources ()
        {
            for (int i = audioTracks.Count - 1; i >= 0; i--)
            {
                var track = audioTracks[i];
                if (track.Playing) continue;
                if (IsOwnedByController(track.Source))
                    sourcesPool.Push(track.Source);
                audioTracks.Remove(track);
            }
        }
    }
}
