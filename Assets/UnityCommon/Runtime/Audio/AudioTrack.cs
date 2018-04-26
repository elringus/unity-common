using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Represents and audio source with attached clip.
/// </summary>
public class AudioTrack
{
    public string Name { get { return Clip.name; } }
    public AudioClip Clip { get; private set; }
    public AudioSource Source { get; private set; }
    public bool IsValid { get { return Clip && Source; } }
    public bool IsLooped { get { return IsValid ? Source.loop : false; } set { if (IsValid) Source.loop = value; } }
    public bool IsPlaying { get { return IsValid ? Source.isPlaying : false; } }
    public bool IsMuted { get { return IsValid ? Source.mute : false; } set { if (IsValid) Source.mute = value; } }
    public float Volume { get { return IsValid ? Source.volume : 0f; } set { if (IsValid) Source.volume = value; } }

    private Tweener<FloatTween> volumeTweener;

    public AudioTrack (AudioClip clip, AudioSource source, MonoBehaviour behaviourContainer = null, 
        float volume = 1f, bool loop = false, AudioMixerGroup mixerGroup = null)
    {
        Clip = clip;
        Source = source;
        Source.clip = Clip;
        Source.volume = volume;
        Source.loop = loop;
        Source.outputAudioMixerGroup = mixerGroup;

        volumeTweener = new Tweener<FloatTween>(behaviourContainer);
    }

    public void Play ()
    {
        if (!IsValid) return;
        Source.Play();
    }

    public AsyncAction Play (float fadeInTime)
    {
        if (!IsValid) return AsyncAction.CreateCompleted();
        if (volumeTweener.IsRunning)
            volumeTweener.CompleteInstantly();
        if (!IsPlaying) Play();
        var tween = new FloatTween(0, Volume, fadeInTime, volume => Volume = volume, true);
        return volumeTweener.Run(tween);
    }

    public void Stop ()
    {
        if (!IsValid) return;
        Source.Stop();
    }

    public AsyncAction Stop (float fadeOutTime)
    {
        if (!IsValid) return AsyncAction.CreateCompleted();
        if (volumeTweener.IsRunning)
            volumeTweener.CompleteInstantly();
        var tween = new FloatTween(Volume, 0, fadeOutTime, volume => Volume = volume, true);
        return volumeTweener.Run(tween).Then(Stop);
    }

    public AsyncAction Fade (float volume, float fadeTime)
    {
        if (!IsValid) return AsyncAction.CreateCompleted();
        if (volumeTweener.IsRunning)
            volumeTweener.CompleteInstantly();
        var tween = new FloatTween(Volume, volume, fadeTime, v => Volume = v, true);
        return volumeTweener.Run(tween);
    }
}
