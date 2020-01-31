using System;
using System.IO;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Implementation is able to manage serializable data instances (slots).
    /// </summary>
    public interface ISaveSlotManager
    {
        /// <summary>
        /// Event invoked before a save (serialization) operation is started.
        /// </summary>
        event Action OnBeforeSave;
        /// <summary>
        /// Event invoked after a save (serialization) operation is finished.
        /// </summary>
        event Action OnSaved;
        /// <summary>
        /// Event invoked before a load (de-serialization) operation is started.
        /// </summary>
        event Action OnBeforeLoad;
        /// <summary>
        /// Event invoked after a load (de-serialization) operation is finished.
        /// </summary>
        event Action OnLoaded;

        /// <summary>
        /// Whether a save (serialization) operation is currently running.
        /// </summary>
        bool Loading { get; }
        /// <summary>
        /// Whether a load (de-serialization) operation is currently running.
        /// </summary>
        bool Saving { get; }

        /// <summary>
        /// Checks whether a save slot with the provided ID is available.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        bool SaveSlotExists (string slotId);
        /// <summary>
        /// Checks whether any save slot is available.
        /// </summary>
        bool AnySaveExists ();
        /// <summary>
        /// Deletes a save slot with the provided ID.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        void DeleteSaveSlot (string slotId);
        /// <summary>
        /// Renames a save slot from <paramref name="sourceSlotId"/> to <paramref name="destSlotId"/>.
        /// Will overwrite <paramref name="destSlotId"/> slot in case it exists.
        /// </summary>
        /// <param name="sourceSlotId">ID of the slot to rename.</param>
        /// <param name="destSlotId">New ID of the slot.</param>
        void RenameSaveSlot (string sourceSlotId, string destSlotId);
    }

    /// <summary>
    /// Implementation is able to manage serializable data instances (slots) of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">Type of the managed data; should be serializable via Unity's serialization system.</typeparam>
    public interface ISaveSlotManager<TData> : ISaveSlotManager where TData : new()
    {
        /// <summary>
        /// Saves (serializes) provided data under the provided save slot ID.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        /// <param name="data">Data to serialize.</param>
        UniTask SaveAsync (string slotId, TData data);
        /// <summary>
        /// Loads (de-serializes) a save slot with the provided ID;
        /// returns null in case requested save slot doesn't exist.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        UniTask<TData> LoadAsync (string slotId);
        /// <summary>
        /// Loads (de-serializes) a save slot with the provided ID; 
        /// will create a new default <typeparamref name="TData"/> and save it under the provided slot ID in case it doesn't exist.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        UniTask<TData> LoadOrDefaultAsync (string slotId);
    }

    /// <summary>
    /// Manages serializable data instances (slots) using local file system (<see cref="System.IO.File"/>).
    /// </summary>
    public abstract class IOSaveSlotManager : ISaveSlotManager
    {
        public event Action OnBeforeSave;
        public event Action OnSaved;
        public event Action OnBeforeLoad;
        public event Action OnLoaded;

        public bool Loading { get; private set; }
        public bool Saving { get; private set; }

        public abstract bool SaveSlotExists (string slotId);
        public abstract bool AnySaveExists ();
        public abstract void DeleteSaveSlot (string slotId);
        public abstract void RenameSaveSlot (string sourceSlotId, string destSlotId);

        protected abstract bool PrettifyJson { get; }
        protected abstract bool Binary { get; }
        protected abstract string Extension { get; }

        protected void InvokeOnBeforeSave () { Saving = true; OnBeforeSave.SafeInvoke(); }
        protected void InvokeOnSaved () { Saving = false; OnSaved.SafeInvoke(); }
        protected void InvokeOnBeforeLoad () { Loading = true; OnBeforeLoad.SafeInvoke(); }
        protected void InvokeOnLoaded () { Loading = false; OnLoaded.SafeInvoke(); }
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
                await AsyncUtils.WaitEndOfFrame;

            saveInProgress = true;

            InvokeOnBeforeSave();

            await SerializeDataAsync(slotId, data);
            InvokeOnSaved();

            saveInProgress = false;
        }

        public async UniTask<TData> LoadAsync (string slotId)
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
            IOUtils.DeleteFile(SlotIdToFilePath(slotId));
        }

        public override void RenameSaveSlot (string sourceSlotId, string destSlotId)
        {
            if (!SaveSlotExists(sourceSlotId)) return;

            var sourceFilePath = SlotIdToFilePath(sourceSlotId);
            var destFilePath = SlotIdToFilePath(destSlotId);
            IOUtils.MoveFile(sourceFilePath, destFilePath);
        }

        public virtual string SlotIdToFilePath (string slotId) => string.Concat(SaveDataPath, "/", slotId, $".{Extension}");

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
            #if UNITY_STANDALONE || UNITY_EDITOR
            return Application.dataPath;
            #else
            return Application.persistentDataPath;
            #endif
        }
    }
}
