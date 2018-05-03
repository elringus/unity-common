using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages serializable data instances (slots) using <see cref="System.IO"/>.
/// </summary>
public abstract class SaveSlotManager
{
    public event Action OnBeforeSave;
    public event Action OnSaved;
    public event Action OnBeforeLoad;
    public event Action OnLoaded;

    public bool IsLoading { get; private set; }
    public bool IsSaving { get; private set; }

    public abstract bool SaveSlotExists (string slotId);
    public abstract bool AnySaveExists ();

    protected void InvokeOnBeforeSave () { IsSaving = true; OnBeforeSave.SafeInvoke(); }
    protected void InvokeOnSaved () { IsSaving = false; OnSaved.SafeInvoke(); }
    protected void InvokeOnBeforeLoad () { IsLoading = true; OnBeforeLoad.SafeInvoke(); }
    protected void InvokeOnLoaded () { IsLoading = false; OnLoaded.SafeInvoke(); }
}

/// <summary>
/// Manages serializable <see cref="TData"/> instances (slots) using <see cref="System.IO"/>.
/// </summary>
public class SaveSlotManager<TData> : SaveSlotManager where TData : new()
{
    protected virtual string SaveDataPath { get { return string.Concat(Application.dataPath, "/SaveData"); } }

    public AsyncAction Save (string slotId, TData data)
    {
        InvokeOnBeforeSave();

        return SerializeData(slotId, data).Then(InvokeOnSaved);
    }

    public AsyncAction<TData> Load (string slotId)
    {
        InvokeOnBeforeLoad();

        if (!SaveSlotExists(slotId))
        {
            Debug.LogError(string.Format("Slot '{0}' not found when loading '{1}' data.", slotId, typeof(TData)));
            return AsyncAction<TData>.CreateCompleted(default(TData));
        }

        return DeserializeData(slotId).ThenG(InvokeOnLoaded);
    }

    /// <summary>
    /// Same as <see cref="Load(string)"/>, but will create a new default <see cref="TData"/> slot in case it doesn't exist.
    /// </summary>
    public AsyncAction<TData> LoadOrDefault (string slotId)
    {
        if (!SaveSlotExists(slotId))
            return SerializeData(slotId, new TData()).ThenAsync(() => Load(slotId));

        return Load(slotId);
    }

    public override bool SaveSlotExists (string slotId)
    {
        var filePath = string.Concat(SaveDataPath, "/", slotId, ".json");
        return File.Exists(filePath);
    }

    public override bool AnySaveExists ()
    {
        return Directory.GetFiles(SaveDataPath, "*.json", SearchOption.TopDirectoryOnly).Length > 0;
    }

    protected virtual AsyncAction SerializeData (string slotId, TData data)
    {
        // TODO: Use async IO.
        var jsonData = JsonUtility.ToJson(data, Debug.isDebugBuild);
        var filePath = string.Concat(SaveDataPath, "/", slotId, ".json");
        Directory.CreateDirectory(SaveDataPath);
        using (var stream = File.CreateText(filePath))
            stream.Write(jsonData);

        // Flush cached file writes to IndexedDB on WebGL.
        // https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLExtensions.SyncFs();
        #endif

        return AsyncAction.CreateCompleted();
    }

    protected virtual AsyncAction<TData> DeserializeData (string slotId)
    {
        // TODO: Use async IO.
        var filePath = string.Concat(SaveDataPath, "/", slotId, ".json");
        using (var stream = File.OpenText(filePath))
        {
            var jsonData = stream.ReadToEnd();
            var data = JsonUtility.FromJson<TData>(jsonData);
            return AsyncAction<TData>.CreateCompleted(data);
        }
    }
}
