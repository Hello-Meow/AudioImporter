using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A component for importing audio files on Android and iOS.
/// </summary>
[AddComponentMenu("AudioImporter/Mobile Importer")]
public class MobileImporter : AudioImporter
{
    private UnityWebRequest webRequest;
    private UnityWebRequestAsyncOperation operation;
    
    public override float progress
    {
        get
        {
            if (operation == null)
                return 0;

            return operation.progress;
        }
    }

    public override bool isDone
    {
        get
        {
            if (operation == null)
                return false;

            return operation.isDone;
        }
    }

    public override bool isError
    {
        get
        {
            if (webRequest == null)
                return false;

            return webRequest.isNetworkError || webRequest.isHttpError;
        }
    }

    public override string error
    {
        get
        {
            if (webRequest == null)
                return string.Empty;

            return webRequest.error;
        }
    }
    
    /// <summary>
    /// Stop importing as soon as possible.
    /// </summary>
    public override void Abort()
    {
        if (webRequest != null)
        {
            webRequest.Abort();
            webRequest.Dispose();
        }    
    }

    protected override void Import()
    {
        webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
        operation = webRequest.SendWebRequest();

        operation.completed += OnCompleted;
    }

    private void OnCompleted(AsyncOperation operation)
    {
        operation.completed -= OnCompleted;

        audioClip = DownloadHandlerAudioClip.GetContent(webRequest);

        webRequest.Dispose();

        OnLoaded();
    }
}
