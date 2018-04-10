using NLayer;
using System;
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
        var bufferLength = mpegFile.SampleRate;
        var samplesBuffer = new float[bufferLength];
        var audioClip = AudioClip.Create("Generated MP3 Audio", (int)mpegFile.SampleCount, mpegFile.Channels, mpegFile.SampleRate, false);
        var sampleOffset = 0;
        while (mpegFile.Position < mpegFile.Length)
        {
            var samplesRead = mpegFile.ReadSamples(samplesBuffer, 0, bufferLength);
            if (samplesRead < bufferLength) Array.Resize(ref samplesBuffer, samplesRead);
            audioClip.SetData(samplesBuffer, (sampleOffset / sizeof(float)) * mpegFile.Channels);
            if (samplesRead < bufferLength) break;
            sampleOffset += samplesRead;
        }
        mpegFile.Dispose();
        return audioClip;
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
