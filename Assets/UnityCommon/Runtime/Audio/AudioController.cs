using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioListener), typeof(AudioSource)), RegisterInContext, SpawnOnContextResolve]
public class AudioController : MonoBehaviour
{
    public AudioListener Listener { get { return audioListener; } }
    public AudioSource MainSource { get { return audioSource; } }
    public bool IsMuted { get { return isMuted; } set { SetIsMuted(value); } }
    public float Volume { get { return lastSetVolume; } set { SetVolume(value); } }

    private AudioListener audioListener;
    private AudioSource audioSource;
    private Dictionary<AudioClip, AudioTrack> audioTracks;
    private Tweener<FloatTween> volumeTweener;
    private FloatTween volumeTween;
    private float lastSetVolume = 1f;
    private bool isMuted;

    private void Awake ()
    {
        audioListener = GetComponent<AudioListener>();
        if (!audioListener) audioListener = gameObject.AddComponent<AudioListener>();
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        audioTracks = new Dictionary<AudioClip, AudioTrack>();
        volumeTweener = new Tweener<FloatTween>(this);
        volumeTween = new FloatTween(0, 0, 0, SetVolume, true);
    }

    public void AttachListener (Transform target)
    {
        transform.SetParent(target);
        transform.localPosition = Vector3.zero;
    }

    public void SetIsMuted (bool isMuted)
    {
        this.isMuted = isMuted;
        AudioListener.volume = isMuted ? 0 : lastSetVolume;
    }

    public void SetVolume (float volume)
    {
        lastSetVolume = Mathf.Clamp01(volume);
        if (!IsMuted) AudioListener.volume = volume;
    }

    public void FadeVolume (float volume, float time)
    {
        if (IsMuted)
        {
            Volume = volume;
            return;
        }
        volumeTween.StartValue = Volume;
        volumeTween.TargetValue = volume;
        volumeTween.TweenDuration = time;
        volumeTweener.Run(volumeTween);
    }

    public void Play2D (AudioClip clip, bool loop = false)
    {
        if (!clip) return;
        audioSource.PlayOneShot(clip);
        audioSource.loop = loop;
    }

    public void Play3D (AudioClip clip, AudioSource source, bool loop = false)
    {
        if (!clip) return;
        source.PlayOneShot(clip);
        source.loop = loop;
    }

    public void Play3D (AudioClip clip, GameObject sourceObject, bool loop = false)
    {
        var source = sourceObject.GetComponent<AudioSource>();
        if (!source)
        {
            source = sourceObject.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
        }
        Play3D(clip, source, loop);
    }

    public AudioTrack AddTrack (AudioClip clip, AudioSource source = null, float volume = 1f, bool loop = false)
    {
        if (audioTracks.ContainsKey(clip)) return audioTracks[clip];
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        var track = new AudioTrack(clip, source, volume, loop);
        audioTracks.Add(clip, track);
        return track;
    }

    public void RemoveTrack (AudioClip clip)
    {
        if (MainSource.clip == clip)
            MainSource.clip = null;

        if (audioTracks.ContainsKey(clip))
        {
            var track = GetTrack(clip);
            if (track.Source.isPlaying)
                track.Source.Stop();
            Destroy(track.Source);
            audioTracks.Remove(clip);
        }
    }

    public void RemoveTrack (string clipName)
    {
        var track = GetTrack(clipName);
        if (track != null) RemoveTrack(track.Clip);
    }

    public void RemoveAllTracks ()
    {
        audioSource.clip = null;
        var clips = audioTracks.Keys.ToList();
        foreach (var clip in clips)
            RemoveTrack(clip);
    }

    public AudioTrack GetTrack (AudioClip clip)
    {
        return audioTracks.ContainsKey(clip) ? audioTracks[clip] : null;
    }

    public AudioTrack GetTrack (string clipName)
    {
        var clip = audioTracks.Keys.ToList().FirstOrDefault(c => c.name == clipName);
        if (!clip) return null;
        return audioTracks[clip];
    }

    public AsyncAction FadeIn (AudioClip clip, float time, bool loop = false)
    {
        var track = audioTracks.ContainsKey(clip) ? audioTracks[clip] : AddTrack(clip);
        track.Source.loop = loop;
        if (!track.Source.isPlaying) track.Source.Play();
        var tweener = new Tweener<FloatTween>(this);
        var tween = new FloatTween(track.Source.volume, 1, time, volume => track.Source.volume = volume, true);
        return tweener.Run(tween);
    }

    public AsyncAction FadeOut (AudioClip clip, float time)
    {
        if (!audioTracks.ContainsKey(clip))
        {
            Debug.LogError(string.Format("Failed to fade-out clip '{0}': track with this clip wasn't added to the audio controller.", clip.name));
            return AsyncAction.CreateCompleted();
        }
        var track = audioTracks[clip];
        var tweener = new Tweener<FloatTween>(this);
        var tween = new FloatTween(track.Source.volume, 0, time, volume => track.Source.volume = volume, true);
        return tweener.Run(tween).Then(track.Source.Stop);
    }

    /// <summary>
    /// Fade-out all playing tracks.
    /// </summary>
    public AsyncAction FadeAllOut (float time)
    {
        return new AsyncActionSet(audioTracks
            .Where(a => a.Value.Source.isPlaying)
            .Select(a => FadeOut(a.Key, time)).ToArray());
    }
}
