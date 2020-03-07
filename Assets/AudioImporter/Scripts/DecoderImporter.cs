using System;
using System.Collections.Generic;
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

    private bool abort;

    private Queue<Action> executionQueue = new Queue<Action>();
    private object _lock = new object();

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

        if(!isInitialized)
            Destroy(audioClip);

        lock (_lock)
            executionQueue.Clear();

        waitForMainThread.Set();
        
        import.Join();
    }

    protected override void Import()
    {
        bufferSize = 2048 * 128;
        buffer = new float[bufferSize];

        isDone = false;
        isInitialized = false;
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

        Dispatch(CreateClip);
        Decode();
        Cleanup();

        progress = 1;
        isDone = true;
    }

    private void Decode()
    {
        while (index < info.lengthSamples)
        {
            int read = GetSamples(buffer, 0, bufferSize);
            
            if(read == 0)
                break;

            if (abort)
                break;

            if (index + bufferSize >= info.lengthSamples)
                Array.Resize(ref buffer, info.lengthSamples - index);

            Dispatch(SetData);

            index += read;

            progress = (float)index / info.lengthSamples;
        }
    }

    private void CreateClip()
    {
        string name = Path.GetFileNameWithoutExtension(uri.LocalPath);

        audioClip = AudioClip.Create(name, info.lengthSamples / info.channels, info.channels, info.sampleRate, false);

        waitForMainThread.Set();
    }

    private void SetData()
    {
        if (audioClip == null)
        {
            Abort();
            return;
        }

        audioClip.SetData(buffer, index / info.channels);

        if(!isInitialized)
        {
            isInitialized = true;
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

    private void Dispatch(Action action)
    {
        lock (_lock)
            executionQueue.Enqueue(action);

        waitForMainThread.WaitOne();
    }

    void Update()
    {        
        lock(_lock)
        {
            while(executionQueue.Count > 0)            
                executionQueue.Dequeue().Invoke();
        }
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