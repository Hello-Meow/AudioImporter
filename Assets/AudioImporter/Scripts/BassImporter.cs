using System;
using Un4seen.Bass;
using UnityEngine;

/// <summary>
/// A componen for importing audio files using BASS and BASS.NET.
/// </summary>
[AddComponentMenu("AudioImporter/Bass Importer")]
public class BassImporter : DecoderImporter
{
    private int handle = -1;

    private float[] offsetBuffer;

    static BassImporter()
    {
        Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    protected override void Initialize()
    {
        if (uri.IsFile)
            handle = Bass.BASS_StreamCreateFile(uri.LocalPath, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
        else
            handle = Bass.BASS_StreamCreateURL(uri.AbsoluteUri, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE, null, default(IntPtr));

        BASSError error = Bass.BASS_ErrorGetCode();
        if (error != BASSError.BASS_OK && error != BASSError.BASS_ERROR_ALREADY)
            OnError(error.ToString());
    }

    protected override void Cleanup()
    {
        if (handle != -1)
            Bass.BASS_StreamFree(handle);
    }

    protected override AudioInfo GetInfo()
    {
        BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(handle);

        int lengthSamples = (int)Bass.BASS_ChannelGetLength(handle) / sizeof(float);

        return new AudioInfo(lengthSamples, info.freq, info.chans);
    }

    protected override int GetSamples(float[] buffer, int offset, int count)
    {
        if (offsetBuffer == null || offsetBuffer.Length != count)
            offsetBuffer = new float[count];

        int read = Bass.BASS_ChannelGetData(handle, offsetBuffer, Math.Min(count, buffer.Length - offset) * sizeof(float));
        read /= sizeof(float);
        Array.Copy(offsetBuffer, 0, buffer, offset, read);

        return read;
    }
}