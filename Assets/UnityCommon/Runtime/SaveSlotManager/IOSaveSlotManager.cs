using System;
using System.IO;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Manages serializable data instances (slots) using local file system (<see cref="System.IO.File"/>).
    /// </summary>
    public abstract class IOSaveSlotManager : ISaveSlotManager
    {
        public event Action<string> OnBeforeSave;
        public event Action<string> OnSaved;
        public event Action<string> OnBeforeLoad;
        public event Action<string> OnLoaded;
        public event Action<string> OnBeforeDelete;
        public event Action<string> OnDeleted;
        public event Action<string, string> OnBeforeRename;
        public event Action<string, string> OnRenamed;

        public bool Loading { get; private set; }
        public bool Saving { get; private set; }

        public abstract bool SaveSlotExists (string slotId);
        public abstract bool AnySaveExists ();
        public abstract void DeleteSaveSlot (string slotId);
        public abstract void RenameSaveSlot (string sourceSlotId, string destSlotId);

        protected abstract bool PrettifyJson { get; }
        protected abstract bool Binary { get; }
        protected abstract string Extension { get; }

        protected void InvokeOnBeforeSave (string slotId)
        {
            Saving = true;
            OnBeforeSave?.Invoke(slotId);
        }

        protected void InvokeOnSaved (string slotId)
        {
            Saving = false;
            OnSaved?.Invoke(slotId);
        }

        protected void InvokeOnBeforeLoad (string slotId)
        {
            Loading = true;
            OnBeforeLoad?.Invoke(slotId);
        }

        protected void InvokeOnLoaded (string slotId)
        {
            Loading = false;
            OnLoaded?.Invoke(slotId);
        }
        
        protected void InvokeOnBeforeDelete (string slotId)
        {
            OnBeforeDelete?.Invoke(slotId);
        }

        protected void InvokeOnDeleted (string slotId)
        {
            OnDeleted?.Invoke(slotId);
        }
        
        protected void InvokeOnBeforeRename (string sourceSlotId, string destSlotId)
        {
            OnBeforeRename?.Invoke(sourceSlotId, destSlotId);
        }

        protected void InvokeOnRenamed (string sourceSlotId, string destSlotId)
        {
            OnRenamed?.Invoke(sourceSlotId, destSlotId);
        }
    }

    /// <summary>
    /// Manages serializable <typeparamref name="TData"/> instances (slots) using local file system (<see cref="System.IO.File"/>).
    /// </summary>
    public class IOSaveSlotManager<TData> : IOSaveSlotManager, ISaveSlotManager<TData> where TData : new()
    {
        protected virtual string GameDataPath => GetGameDataPath();
        protected virtual string SaveDataPath => string.Concat(GameDataPath, "/SaveData");
        protected override bool PrettifyJson => Debug.isDebugBuild;
        protected override bool Binary => false;
        protected override string Extension => "json";

        private bool saveInProgress;

        public async UniTask SaveAsync (string slotId, TData data)
        {
            while (saveInProgress && Application.isPlaying)
                await AsyncUtils.WaitEndOfFrameAsync();

            saveInProgress = true;

            InvokeOnBeforeSave(slotId);

            await SerializeDataAsync(slotId, data);
            InvokeOnSaved(slotId);

            saveInProgress = false;
        }

        public async UniTask<TData> LoadAsync (string slotId)
        {
            InvokeOnBeforeLoad(slotId);

            if (!SaveSlotExists(slotId))
                throw new Error($"Slot '{slotId}' not found when loading '{typeof(TData)}' data.");

            var data = await DeserializeDataAsync(slotId);
            InvokeOnLoaded(slotId);

            return data;
        }

        public async UniTask<TData> LoadOrDefaultAsync (string slotId)
        {
            if (!SaveSlotExists(slotId))
                await SerializeDataAsync(slotId, new TData());

            return await LoadAsync(slotId);
        }

        public override bool SaveSlotExists (string slotId) => File.Exists(SlotIdToFilePath(slotId));

        public override bool AnySaveExists ()
        {
            if (!Directory.Exists(SaveDataPath)) return false;
            return Directory.GetFiles(SaveDataPath, $"*.{Extension}", SearchOption.TopDirectoryOnly).Length > 0;
        }

        public override void DeleteSaveSlot (string slotId)
        {
            if (!SaveSlotExists(slotId)) return;
            InvokeOnBeforeDelete(slotId);
            IOUtils.DeleteFile(SlotIdToFilePath(slotId));
            InvokeOnDeleted(slotId);
        }

        public override void RenameSaveSlot (string sourceSlotId, string destSlotId)
        {
            if (!SaveSlotExists(sourceSlotId)) return;

            InvokeOnBeforeRename(sourceSlotId, destSlotId);
            var sourceFilePath = SlotIdToFilePath(sourceSlotId);
            var destFilePath = SlotIdToFilePath(destSlotId);
            IOUtils.MoveFile(sourceFilePath, destFilePath);
            InvokeOnRenamed(sourceSlotId, destSlotId);
        }

        protected virtual string SlotIdToFilePath (string slotId) => string.Concat(SaveDataPath, "/", slotId, $".{Extension}");

        protected virtual async UniTask SerializeDataAsync (string slotId, TData data)
        {
            var jsonData = JsonUtility.ToJson(data, PrettifyJson);
            var filePath = SlotIdToFilePath(slotId);
            IOUtils.CreateDirectory(SaveDataPath);

            if (Binary)
            {
                var bytes = await StringUtils.ZipStringAsync(jsonData);
                await IOUtils.WriteFileAsync(filePath, bytes);
            }
            else await IOUtils.WriteTextFileAsync(filePath, jsonData);
        }

        protected virtual async UniTask<TData> DeserializeDataAsync (string slotId)
        {
            var filePath = SlotIdToFilePath(slotId);
            var jsonData = default(string);

            if (Binary)
            {
                var bytes = await IOUtils.ReadFileAsync(filePath);
                jsonData = await StringUtils.UnzipStringAsync(bytes);
            }
            else jsonData = await IOUtils.ReadTextFileAsync(filePath);

            return JsonUtility.FromJson<TData>(jsonData);
        }

        protected virtual string GetGameDataPath ()
        {
            #if UNITY_EDITOR
            return Application.dataPath;
            #else
            return Application.persistentDataPath;
            #endif
        }
    }
}
