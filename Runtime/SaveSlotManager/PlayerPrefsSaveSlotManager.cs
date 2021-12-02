using System;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Manages serializable data instances (slots) using <see cref="UnityEngine.PlayerPrefs"/>.
    /// </summary>
    public abstract class PlayerPrefsSaveSlotManager : ISaveSlotManager
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
    /// Manages serializable <typeparamref name="TData"/> instances (slots) using <see cref="UnityEngine.PlayerPrefs"/>.
    /// </summary>
    public class PlayerPrefsSaveSlotManager<TData> : PlayerPrefsSaveSlotManager, ISaveSlotManager<TData> where TData : new()
    {
        protected virtual string KeyPrefix => GetType().FullName;
        protected virtual string IndexKey => KeyPrefix + "Index";
        protected virtual string IndexDelimiter => "|";
        protected override bool PrettifyJson => Debug.isDebugBuild;
        protected override bool Binary => false;

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

        public void Save (string slotId, TData data)
        {
            saveInProgress = true;
            InvokeOnBeforeSave(slotId);
            SerializeData(slotId, data);
            InvokeOnSaved(slotId);
            saveInProgress = false;
        }

        public async UniTask<TData> LoadAsync (string slotId)
        {
            InvokeOnBeforeLoad(slotId);

            if (!SaveSlotExists(slotId))
                throw new Exception($"Slot '{slotId}' not found when loading '{typeof(TData)}' data.");

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

        public override bool SaveSlotExists (string slotId) => PlayerPrefs.HasKey(SlotIdToKey(slotId));

        public override bool AnySaveExists () => !string.IsNullOrEmpty(PlayerPrefs.GetString(IndexKey));

        public override void DeleteSaveSlot (string slotId)
        {
            if (!SaveSlotExists(slotId)) return;

            InvokeOnBeforeDelete(slotId);
            var slotKey = SlotIdToKey(slotId);
            PlayerPrefs.DeleteKey(slotKey);
            RemoveKeyIndex(slotKey);
            PlayerPrefs.Save();
            InvokeOnDeleted(slotId);
        }

        public override void RenameSaveSlot (string sourceSlotId, string destSlotId)
        {
            if (!SaveSlotExists(sourceSlotId)) return;

            var sourceKey = SlotIdToKey(sourceSlotId);
            var destKey = SlotIdToKey(destSlotId);
            var sourceValue = PlayerPrefs.GetString(sourceKey);

            InvokeOnBeforeRename(sourceSlotId, destSlotId);
            DeleteSaveSlot(sourceSlotId);
            PlayerPrefs.SetString(destKey, sourceValue);
            AddKeyIndexIfNotExist(destKey);
            PlayerPrefs.Save();
            InvokeOnRenamed(sourceSlotId, destSlotId);
        }

        protected virtual string SlotIdToKey (string slotId) => KeyPrefix + slotId;

        protected virtual async UniTask SerializeDataAsync (string slotId, TData data)
        {
            var jsonData = JsonUtility.ToJson(data, PrettifyJson);
            var slotKey = SlotIdToKey(slotId);

            if (Binary)
            {
                var bytes = await StringUtils.ZipStringAsync(jsonData);
                jsonData = Convert.ToBase64String(bytes);
            }

            PlayerPrefs.SetString(slotKey, jsonData);
            AddKeyIndexIfNotExist(slotKey);
            PlayerPrefs.Save();
        }

        protected virtual void SerializeData (string slotId, TData data)
        {
            var jsonData = JsonUtility.ToJson(data, PrettifyJson);
            var slotKey = SlotIdToKey(slotId);

            if (Binary)
            {
                var bytes = StringUtils.ZipString(jsonData);
                jsonData = Convert.ToBase64String(bytes);
            }

            PlayerPrefs.SetString(slotKey, jsonData);
            AddKeyIndexIfNotExist(slotKey);
            PlayerPrefs.Save();
        }

        protected virtual async UniTask<TData> DeserializeDataAsync (string slotId)
        {
            var slotKey = SlotIdToKey(slotId);
            var jsonData = default(string);

            if (Binary)
            {
                var base64 = PlayerPrefs.GetString(slotKey);
                var bytes = Convert.FromBase64String(base64);
                jsonData = await StringUtils.UnzipStringAsync(bytes);
            }
            else jsonData = PlayerPrefs.GetString(slotKey);

            return JsonUtility.FromJson<TData>(jsonData);
        }

        protected virtual void AddKeyIndexIfNotExist (string slotKey)
        {
            if (!PlayerPrefs.HasKey(IndexKey))
                PlayerPrefs.SetString(IndexKey, string.Empty);

            var indexList = PlayerPrefs.GetString(IndexKey).Split(new[] { IndexDelimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (indexList.Exists(i => i == slotKey)) return;

            indexList.Add(slotKey);
            var index = string.Join(IndexDelimiter, indexList);
            PlayerPrefs.SetString(IndexKey, index);
        }

        protected virtual void RemoveKeyIndex (string slotKey)
        {
            if (!PlayerPrefs.HasKey(IndexKey)) return;

            var indexList = PlayerPrefs.GetString(IndexKey).Split(new[] { IndexDelimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!indexList.Remove(slotKey)) return;

            var index = string.Join(IndexDelimiter, indexList);
            PlayerPrefs.SetString(IndexKey, index);
        }
    }
}
