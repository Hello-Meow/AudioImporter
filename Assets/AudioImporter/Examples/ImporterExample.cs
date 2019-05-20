using System.Collections;
using UnityEngine;

public class ImporterExample : MonoBehaviour
{
    public Browser browser;
    public AudioImporter importer;
    public AudioSource audioSource;

    void Awake()
    {
        browser.FileSelected += OnFileSelected;
    }

    private void OnFileSelected(string path)
    {
        if (importer.isDone)
            Destroy(audioSource.clip);

        StartCoroutine(Import(path));
    }

    IEnumerator Import(string path)
    {
        importer.Import(path);

        while (!importer.isDone)
            yield return null;

        audioSource.clip = importer.audioClip;
        audioSource.Play();
    }
}
