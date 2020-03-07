using System;
using UnityEngine;

/// <summary>
/// A base class for importing audio files.
/// </summary>
public abstract class AudioImporter : MonoBehaviour
{
    /// <summary>
    /// Occurs when the importer has loaded an AudioClip.
    /// </summary>
    public event Action<AudioClip> Loaded;

    /// <summary>
    /// The uri of the file being imported.
    /// </summary>
    public Uri uri { get; private set; }

    /// <summary>
    /// The AudioClip of the file that is being imported.
    /// </summary>
    public virtual AudioClip audioClip { get; protected set; }

    /// <summary>
    /// The current progress of the importer, ranging from 0-1.
    /// </summary>
    public virtual float progress { get; protected set; }

    /// <summary>
    /// Is the full audio file imported?
    /// </summary>
    public virtual bool isDone { get; protected set; }

    /// <summary>
    /// When the importer is initialized, the AudioClip is available. Importing might continue in the background.
    /// </summary>
    public virtual bool isInitialized { get; protected set; }

    /// <summary>
    /// Has an error occured?
    /// </summary>
    public virtual bool isError { get; protected set; }

    /// <summary>
    /// An error message, if an error has occured.
    /// </summary>
    public virtual string error { get; protected set; }

    /// <summary>
    /// Import an audio file.
    /// </summary>
    /// <param name="uri">The uri to the audio file.</param>
    public void Import(string uri)
    {
        Abort();

        this.uri = new Uri(uri);

        isError = false;
        error = string.Empty;
        
        Import();
    }

    /// <summary>
    /// Stop importing as soon as possible.
    /// </summary>
    public abstract void Abort();

    protected abstract void Import();

    protected void OnLoaded()
    {
        if (Loaded != null)
            Loaded(audioClip);
    }    
}
