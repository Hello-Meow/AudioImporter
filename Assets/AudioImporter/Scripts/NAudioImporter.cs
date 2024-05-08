using System;
using NAudio.Wave;
using UnityEngine;

/// <summary>
/// A component for importing audio files using NAudio.
/// </summary>
[AddComponentMenu("AudioImporter/NAudio Importer")]
public class NAudioImporter : DecoderImporter
{
    private WaveStream waveStream;
    private ISampleProvider sampleProvider;
    
    protected override void Initialize()
    {
        try
        {
            if (!uri.IsFile)
                throw new FormatException("NAudioImporter does not support URLs");

            waveStream = new AudioFileReader(uri.LocalPath);
            sampleProvider = waveStream.ToSampleProvider();
        }
        catch (Exception e)
        {
            OnError(e.Message);
        }
    }

    protected override void Cleanup()
    {
        if (waveStream != null)
            waveStream.Dispose();

        waveStream = null;
        sampleProvider = null;
    }

    protected override AudioInfo GetInfo()
    {
        WaveFormat format = waveStream.WaveFormat;
        int lengthSamples = (int)waveStream.Length / (format.BitsPerSample / 8);
        return new AudioInfo(lengthSamples, format.SampleRate, format.Channels);
    }

    protected override int GetSamples(float[] buffer, int offset, int count)
    {
        return sampleProvider.Read(buffer, offset, count);
    }
}
