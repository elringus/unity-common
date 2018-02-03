using UnityEngine;

/// <summary>
/// Represents and audio source with attached clip.
/// </summary>
public class AudioTrack
{
    public AudioClip Clip { get; private set; }
    public AudioSource Source { get; private set; }

    public AudioTrack (AudioClip clip, AudioSource source, float volume = 1f, bool loop = false)
    {
        Clip = clip;
        Source = source;
        Source.clip = Clip;
        Source.volume = volume;
        Source.loop = loop;
    }
}
