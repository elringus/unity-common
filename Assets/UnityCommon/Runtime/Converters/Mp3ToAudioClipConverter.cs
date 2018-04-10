using NLayer;
using System.IO;
using UnityEngine;

/// <summary>
/// Converts <see cref="byte[]"/> raw data of a .mp3 audio file to <see cref="AudioClip"/>.
/// </summary>
public class Mp3ToAudioClipConverter : IRawConverter<AudioClip>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation("mp3", "audio/mpeg")
    }; } }

    public AudioClip Convert (byte[] obj)
    {
        var mpegFile = new MpegFile(new MemoryStream(obj));
        var samplesCount = (int)mpegFile.SampleCount * mpegFile.Channels;
        var samples = new float[samplesCount];
        var audioClip = AudioClip.Create("Generated MP3 Audio", (int)mpegFile.SampleCount, mpegFile.Channels, mpegFile.SampleRate, false);
        mpegFile.ReadSamples(samples, 0, samplesCount);
        audioClip.SetData(samples, 0);
        mpegFile.Dispose();
        return audioClip;
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
