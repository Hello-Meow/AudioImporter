using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A component for importing audio files on Android and iOS.
/// </summary>
[AddComponentMenu("AudioImporter/Mobile Importer")]
public class MobileImporter : AudioImporter
{
    private UnityWebRequest webRequest;
    private AsyncOperation operation;
    
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

    public override bool isInitialized
    {
        get
        {
            return isDone;
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
            webRequest = null;

            StopAllCoroutines();
        }    
    }

    protected override void Import()
    {
        webRequest = UnityWebRequestMultimedia.GetAudioClip(uri.AbsoluteUri, AudioType.UNKNOWN);
        operation = webRequest.SendWebRequest();

        StartCoroutine(WaitForWebRequest());
    }
    
    IEnumerator WaitForWebRequest()
    {
        yield return operation;

        audioClip = DownloadHandlerAudioClip.GetContent(webRequest);

        webRequest.Dispose();
        webRequest = null;

        OnLoaded();
    }
}
