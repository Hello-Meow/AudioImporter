using System;
using System.Threading;
using System.IO;
using UnityEngine;

/// <summary>
/// A base class for importing audio files by using a decoder.
/// </summary>
public abstract class DecoderImporter : AudioImporter
{
    private AudioInfo info;

    private int bufferSize;
    private float[] buffer;

    private AutoResetEvent waitForMainThread;
    private Thread import;

    private int index;

    private bool createClip;
    private bool setData;
    private bool abort;

    /// <summary>
    /// Stop importing as soon as possible.
    /// </summary>
    public override void Abort()
    {
        if (abort)
            return;

        if (import == null || !import.IsAlive)
            return;

        abort = true;
        createClip = false;
        setData = false;

        waitForMainThread.Set();
        
        import.Join();
    }

    protected override void Import()
    {
        bufferSize = 2048 * 128;
        buffer = new float[bufferSize];

        isDone = false;
        abort = false;
        index = 0;
        progress = 0;

        waitForMainThread = new AutoResetEvent(false);
        
        import = new Thread(DoImport);
        import.Start();
    }

    private void DoImport()
    {
        Initialize();

        if (isError)
            return;

        info = GetInfo();

        createClip = true;
        waitForMainThread.WaitOne();

        while (index < info.lengthSamples)
        {
            //TODO: issue where end of stream is reached but index < lengthSamples

            int read = GetSamples(buffer, 0, bufferSize);

            if (read + index >= info.lengthSamples)
                Array.Resize(ref buffer, read);

            if (abort)
                break;

            setData = true;
            waitForMainThread.WaitOne();

            index += read;

            progress = (float)index / info.lengthSamples;
        }

        Cleanup();
    }

    private void CreateClip()
    {
        string name = Path.GetFileNameWithoutExtension(uri);

        audioClip = AudioClip.Create(name, info.lengthSamples / info.channels, info.channels, info.sampleRate, false);

        createClip = false;

        waitForMainThread.Set();
    }

    private void SetData()
    {
        if (audioClip == null)
        {
            Abort();
            return;
        }

        setData = false;

        audioClip.SetData(buffer, index / info.channels);

        if(!isDone)
        {
            isDone = true;
            OnLoaded();
        }

        waitForMainThread.Set();
    }

    protected void OnError(string error)
    {
        this.error = error;
        isError = true;

        progress = 1;
    }

    void Update()
    {
        if (createClip)
            CreateClip();

        if (setData)
            SetData();               
    }

    protected abstract void Initialize();

    protected abstract void Cleanup();

    protected abstract int GetSamples(float[] buffer, int offset, int count);

    protected abstract AudioInfo GetInfo();

    protected class AudioInfo
    {
        public int lengthSamples { get; private set; }
        public int sampleRate { get; private set; }
        public int channels { get; private set; }

        public AudioInfo(int lengthSamples, int sampleRate, int channels)
        {
            this.lengthSamples = lengthSamples;
            this.sampleRate = sampleRate;
            this.channels = channels;
        }
    }
}