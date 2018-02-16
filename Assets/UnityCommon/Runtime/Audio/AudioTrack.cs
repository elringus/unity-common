using UnityEngine;

/// <summary>
/// Represents and audio source with attached clip.
/// </summary>
public class AudioTrack
{
    public string Name { get; private set; }
    public AudioClip Clip { get; private set; }
    public AudioSource Source { get; private set; }
    public bool IsValid { get { return Clip && Source; } }
    public bool IsLooped { get { return IsValid ? Source.loop : false; } set { if (IsValid) Source.loop = value; } }
    public bool IsPlaying { get { return IsValid ? Source.isPlaying : false; } }
    public float Volume { get { return IsValid ? Source.volume : 0f; } set { if (IsValid) Source.volume = value; } }

    private Tweener<FloatTween> volumeTweener;

    public AudioTrack (AudioClip clip, AudioSource source, MonoBehaviour behaviourContainer = null, float volume = 1f, bool loop = false)
    {
        Name = clip.name;
        Clip = clip;
        Source = source;
        Source.clip = Clip;
        Source.volume = volume;
        Source.loop = loop;

        volumeTweener = new Tweener<FloatTween>(behaviourContainer);
    }

    public void Play ()
    {
        if (!IsValid) return;
        Source.Play();
    }

    public void Stop ()
    {
        if (!IsValid) return;
        Source.Stop();
    }

    public AsyncAction FadeIn (float time, bool loop = false)
    {
        if (!IsValid) return AsyncAction.CreateCompleted();
        if (!IsPlaying) Play();
        var tween = new FloatTween(Volume, 1, time, volume => Volume = volume, true);
        return volumeTweener.Run(tween);
    }

    public AsyncAction FadeOut (float time)
    {
        if (!IsValid) return AsyncAction.CreateCompleted();
        var tween = new FloatTween(Volume, 0, time, volume => Volume = volume, true);
        return volumeTweener.Run(tween).Then(Stop);
    }
}
