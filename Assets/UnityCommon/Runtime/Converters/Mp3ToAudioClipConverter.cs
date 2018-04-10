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
        var audioClip = AudioClip.Create("Generated MP3 Audio", (int)mpegFile.SampleCount, mpegFile.Channels, mpegFile.SampleRate, false);

        // AudioClip.SetData with offset is not supported on WebGL, thus we can't use buffering while encoding.
        // Issue: https://trello.com/c/iWL6eBrV/82-webgl-audio-resources-limitation
        #if UNITY_WEBGL && !UNITY_EDITOR
        var samplesCount = (int)mpegFile.SampleCount * mpegFile.Channels;
        var samples = new float[samplesCount];
        mpegFile.ReadSamples(samples, 0, samplesCount);
        audioClip.SetData(samples, 0);
        #else
        var bufferLength = mpegFile.SampleRate;
        var samplesBuffer = new float[bufferLength];
        var sampleOffset = 0;
        while (mpegFile.Position < mpegFile.Length)
        {
            var samplesRead = mpegFile.ReadSamples(samplesBuffer, 0, bufferLength);
            if (samplesRead < bufferLength) Array.Resize(ref samplesBuffer, samplesRead);
            audioClip.SetData(samplesBuffer, (sampleOffset / sizeof(float)) * mpegFile.Channels);
            if (samplesRead < bufferLength) break;
            sampleOffset += samplesRead;
        }
        #endif

        mpegFile.Dispose();

        return audioClip;
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
