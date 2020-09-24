using System;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Converts <see cref="T:byte[]"/> raw data of a .wav audio file to <see cref="AudioClip"/>.
    /// Only PCM16 44100Hz stereo .wav are supported.
    /// </summary>
    public class WavToAudioClipConverter : IRawConverter<AudioClip>
    {
        public RawDataRepresentation[] Representations { get; } = {
            new RawDataRepresentation(".wav", "audio/wav")
        };

        public AudioClip Convert (byte[] obj, string name)
        {
            var floatArr = Pcm16ToFloatArray(obj);

            var audioClip = AudioClip.Create("Generated WAV Audio", floatArr.Length / 2, 2, 44100, false);
            audioClip.name = name;
            audioClip.SetData(floatArr, 0);

            return audioClip;
        }

        public async UniTask<AudioClip> ConvertAsync (byte[] obj, string name)
        {
            var floatArr = await UniTask.Run(() => Pcm16ToFloatArray(obj));

            var audioClip = AudioClip.Create("Generated WAV Audio", floatArr.Length / 2, 2, 44100, false);
            audioClip.name = name;
            audioClip.SetData(floatArr, 0);

            return audioClip;
        }

        public object Convert (object obj, string name) => Convert(obj as byte[], name);

        public async UniTask<object> ConvertAsync (object obj, string name) => await ConvertAsync(obj as byte[], name);

        private static float[] Pcm16ToFloatArray (byte[] input)
        {
            // PCM16 wav usually has 44 byte headers, though not always. 
            // https://stackoverflow.com/questions/19991405/how-can-i-detect-whether-a-wav-file-has-a-44-or-46-byte-header
            const int headerSize = 444;
            var inputSamples = input.Length / 2; // 16 bit input, so 2 bytes per sample.
            var output = new float[inputSamples];
            var outputIndex = 0;
            for (var n = headerSize; n < inputSamples; n++)
            {
                short sample = BitConverter.ToInt16(input, n * 2);
                output[outputIndex++] = sample / 32768f;
            }

            return output;
        }
    }
}
