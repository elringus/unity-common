using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioListener), typeof(AudioSource)), RegisterInContext, SpawnOnContextResolve]
public class AudioController : MonoBehaviour
{
    public bool IsMuted { get { return isMuted; } set { SetIsMuted(value); } }
    public float Volume { get { return lastSetVolume; } set { SetVolume(value); } }

    private AudioListener audioListener;
    private AudioSource audioSource;
    private List<AudioSource> audioTracks;
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

        audioTracks = new List<AudioSource>();
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

    public void Play2D (AudioClip clip)
    {
        if (!clip) return;
        audioSource.PlayOneShot(clip);
    }

    public void Play3D (AudioClip clip, AudioSource source)
    {
        if (!clip) return;
        source.PlayOneShot(clip);
    }

    public void Play3D (AudioClip clip, GameObject sourceObject)
    {
        var source = sourceObject.GetComponent<AudioSource>();
        if (!source)
        {
            source = sourceObject.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
        }
        Play3D(clip, source);
    }

    public AudioSource AddTrack (AudioClip clip, AudioSource source = null, float volume = 1f, bool loop = false)
    {
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        audioTracks.Add(source);
        return source;
    }

    public List<AudioSource> GetTracksByClip (AudioClip clip)
    {
        return audioTracks.FindAll(track => track.clip == clip);
    }

    public void FadeInTrack (AudioSource track, float time)
    {
        var tweener = new Tweener<FloatTween>(this);
        var tween = new FloatTween(track.volume, 1, time, volume => track.volume = volume, true);
        tweener.Run(tween);
        if (!track.isPlaying) track.Play();
    }

    public void FadeOutTrack (AudioSource track, float time)
    {
        var tweener = new Tweener<FloatTween>(this);
        var tween = new FloatTween(track.volume, 0, time, volume => track.volume = volume, true);
        tweener.Run(tween);
    }

    public void CrossfadeTracks (AudioSource from, AudioSource to, float time)
    {
        FadeOutTrack(from, time);
        FadeInTrack(to, time);
    }

}
