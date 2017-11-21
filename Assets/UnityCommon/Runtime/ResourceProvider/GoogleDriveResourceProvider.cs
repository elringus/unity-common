using System;
using UnityEngine;

/// <summary>
/// Provides resources stored in Google Drive using <a href="https://github.com/Elringus/UnityGoogleDrive">UnityGoogleDrive SDK</a>.
/// </summary>
public class GoogleDriveResourceProvider : MonoRunnerResourceProvider
{
    public override AsyncRunner CreateLoadRunner<T> (string path, Action<string, T> onLoaded = null)
    {
        return new GoogleDriveResourceLoader<T>(path, this, onLoaded);
    }

    public override T GetResourceBlocking<T> (string path)
    {
        Debug.LogError("GoogleDriveResourceProvider doesn't support blocking resource loading.");
        return null;
    }
}
