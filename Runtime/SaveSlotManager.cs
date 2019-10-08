using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
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
        public abstract void DeleteSaveSlot (string slotId);

        protected abstract bool PrettifyJson { get; }
        protected abstract bool Binary { get; }
        protected abstract string Extension { get; }

        protected void InvokeOnBeforeSave () { IsSaving = true; OnBeforeSave.SafeInvoke(); }
        protected void InvokeOnSaved () { IsSaving = false; OnSaved.SafeInvoke(); }
        protected void InvokeOnBeforeLoad () { IsLoading = true; OnBeforeLoad.SafeInvoke(); }
        protected void InvokeOnLoaded () { IsLoading = false; OnLoaded.SafeInvoke(); }
    }

    /// <summary>
    /// Manages serializable <typeparamref name="TData"/> instances (slots) using <see cref="System.IO"/>.
    /// </summary>
    public class SaveSlotManager<TData> : SaveSlotManager where TData : new()
    {
        protected virtual string GameDataPath => GetGameDataPath();
        protected virtual string SaveDataPath => string.Concat(GameDataPath, "/SaveData");
        protected override bool PrettifyJson => Debug.isDebugBuild;
        protected override bool Binary => false;
        protected override string Extension => "json";

        private static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private bool saveInProgress;

        public async Task SaveAsync (string slotId, TData data)
        {
            while (saveInProgress && Application.isPlaying)
                await waitForEndOfFrame;

            saveInProgress = true;

            InvokeOnBeforeSave();

            await SerializeDataAsync(slotId, data);
            InvokeOnSaved();

            saveInProgress = false;
        }

        public async Task<TData> LoadAsync (string slotId)
        {
            InvokeOnBeforeLoad();

            if (!SaveSlotExists(slotId))
            {
                Debug.LogError($"Slot '{slotId}' not found when loading '{typeof(TData)}' data.");
                return default;
            }

            var data = await DeserializeDataAsync(slotId);
            InvokeOnLoaded();

            return data;
        }

        /// <summary>
        /// Same as <see cref="LoadAsync(string)"/>, but will create a new default <typeparamref name="TData"/> slot in case it doesn't exist.
        /// </summary>
        public async Task<TData> LoadOrDefaultAsync (string slotId)
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
            IOUtils.DeleteFile(SlotIdToFilePath(slotId));
        }

        public virtual string SlotIdToFilePath (string slotId) => string.Concat(SaveDataPath, "/", slotId, $".{Extension}");

        protected virtual async Task SerializeDataAsync (string slotId, TData data)
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

        protected virtual async Task<TData> DeserializeDataAsync (string slotId)
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
            #if UNITY_STANDALONE || UNITY_EDITOR
            return Application.dataPath;
            #else
            return Application.persistentDataPath;
            #endif
        }
    }
}
