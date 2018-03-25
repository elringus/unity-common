using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Allows asynchronously reading/writing arbitrary data in JSON format to a persistent storage.
/// Serializable fields in the derived classes will be subject to serialization.
/// Will use <see cref="System.IO"/> when available; <see cref="PlayerPrefs"/> otherwise.
/// </summary>
[Serializable]
public abstract class SerializableState
{
    public event Action OnBeforeSave;
    public event Action OnSaved;
    public event Action OnBeforeLoad;
    public event Action OnLoaded;

    protected virtual bool IsIoSupported { get { return Application.isEditor || (!Application.isMobilePlatform && Application.platform != RuntimePlatform.WebGLPlayer); } }
    protected abstract string SaveDataPath { get; }
    protected abstract string DefaultSlotId { get; }

    public virtual bool SaveSlotExists (string slotId = null)
    {
        if (string.IsNullOrEmpty(slotId))
            slotId = DefaultSlotId;

        if (IsIoSupported)
        {
            var filePath = SaveDataPath + slotId + ".json";
            return File.Exists(filePath);
        }
        else return PlayerPrefs.HasKey(slotId);
    }

    public virtual AsyncAction Load (string slotId = null)
    {
        InvokeOnBeforeLoad();

        if (string.IsNullOrEmpty(slotId))
            slotId = DefaultSlotId;

        if (!SaveSlotExists(slotId))
        {
            Debug.LogError(string.Format("Slot '{0}' not found when loading serializable state.", slotId));
            return AsyncAction.CreateCompleted();
        }

        return DeserializeState(slotId).Then(InvokeOnLoaded);
    }

    public virtual AsyncAction Save (string slotId = null)
    {
        InvokeOnBeforeSave();

        if (string.IsNullOrEmpty(slotId))
            slotId = DefaultSlotId;

        return SerializeState(slotId).Then(InvokeOnSaved);
    }

    protected void InvokeOnBeforeSave () { OnBeforeSave.SafeInvoke(); }
    protected void InvokeOnSaved () { OnSaved.SafeInvoke(); }
    protected void InvokeOnBeforeLoad () { OnBeforeLoad.SafeInvoke(); }
    protected void InvokeOnLoaded () { OnLoaded.SafeInvoke(); }

    protected virtual AsyncAction SerializeState (string slotId)
    {
        if (IsIoSupported)
        {
            // TODO: Use async IO.
            var filePath = SaveDataPath + slotId + ".json";
            var jsonData = JsonUtility.ToJson(this, Debug.isDebugBuild);
            Directory.CreateDirectory(SaveDataPath);
            using (var stream = File.CreateText(filePath))
                stream.Write(jsonData);
        }
        else
        {
            var jsonData = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(slotId, jsonData);
        }

        return AsyncAction.CreateCompleted();
    }

    protected virtual AsyncAction DeserializeState (string slotId)
    {
        if (IsIoSupported)
        {
            // TODO: Use async IO.
            var filePath = SaveDataPath + slotId + ".json";
            using (var stream = File.OpenText(filePath))
            {
                var jsonData = stream.ReadToEnd();
                JsonUtility.FromJsonOverwrite(jsonData, this);
            }
        }
        else
        {
            var jsonData = PlayerPrefs.GetString(slotId);
            JsonUtility.FromJsonOverwrite(jsonData, this);
        }

        return AsyncAction.CreateCompleted();
    }
}
