using System;
using System.Collections.Generic;

namespace UnityCommon
{
    /// <summary>
    /// Manages serializable data instances (slots) in-memory, w/o actually persisting the data.
    /// Can be useful for automated testing and various integration scenarios.
    /// </summary>
    public abstract class TransientSaveSlotManager : ISaveSlotManager
    {
        public event Action<string> OnBeforeSave;
        public event Action<string> OnSaved;
        public event Action<string> OnBeforeLoad;
        public event Action<string> OnLoaded;
        public event Action<string> OnBeforeDelete;
        public event Action<string> OnDeleted;
        public event Action<string, string> OnBeforeRename;
        public event Action<string, string> OnRenamed;

        public virtual bool Loading { get; private set; }
        public virtual bool Saving { get; private set; }

        public abstract bool SaveSlotExists (string slotId);
        public abstract bool AnySaveExists ();
        public abstract void DeleteSaveSlot (string slotId);
        public abstract void RenameSaveSlot (string sourceSlotId, string destSlotId);

        protected virtual void InvokeOnBeforeSave (string slotId)
        {
            Saving = true;
            OnBeforeSave?.Invoke(slotId);
        }

        protected virtual void InvokeOnSaved (string slotId)
        {
            Saving = false;
            OnSaved?.Invoke(slotId);
        }

        protected virtual void InvokeOnBeforeLoad (string slotId)
        {
            Loading = true;
            OnBeforeLoad?.Invoke(slotId);
        }

        protected virtual void InvokeOnLoaded (string slotId)
        {
            Loading = false;
            OnLoaded?.Invoke(slotId);
        }

        protected virtual void InvokeOnBeforeDelete (string slotId)
        {
            OnBeforeDelete?.Invoke(slotId);
        }

        protected virtual void InvokeOnDeleted (string slotId)
        {
            OnDeleted?.Invoke(slotId);
        }

        protected virtual void InvokeOnBeforeRename (string sourceSlotId, string destSlotId)
        {
            OnBeforeRename?.Invoke(sourceSlotId, destSlotId);
        }

        protected virtual void InvokeOnRenamed (string sourceSlotId, string destSlotId)
        {
            OnRenamed?.Invoke(sourceSlotId, destSlotId);
        }
    }

    /// <inheritdoc cref="TransientSaveSlotManager"/>
    public class TransientSaveSlotManager<TData> : TransientSaveSlotManager, ISaveSlotManager<TData> where TData : new()
    {
        public virtual IReadOnlyCollection<string> ExistingSlotIds => SlotToData.Keys;

        protected virtual Dictionary<string, TData> SlotToData { get; } = new Dictionary<string, TData>();

        public virtual void Save (string slotId, TData data)
        {
            InvokeOnBeforeSave(slotId);
            SlotToData[slotId] = data;
            InvokeOnSaved(slotId);
        }

        public virtual TData Load (string slotId)
        {
            InvokeOnBeforeLoad(slotId);
            if (!SaveSlotExists(slotId))
                throw new Error($"Slot '{slotId}' not found when loading '{typeof(TData)}' data.");
            InvokeOnLoaded(slotId);
            return SlotToData[slotId];
        }

        public virtual TData LoadOrDefault (string slotId)
        {
            if (!SaveSlotExists(slotId)) Save(slotId, new TData());
            return Load(slotId);
        }

        public virtual UniTask SaveAsync (string slotId, TData data)
        {
            Save(slotId, data);
            return UniTask.CompletedTask;
        }

        public virtual UniTask<TData> LoadAsync (string slotId) => UniTask.FromResult(Load(slotId));
        public virtual UniTask<TData> LoadOrDefaultAsync (string slotId) => UniTask.FromResult(LoadOrDefault(slotId));
        public override bool SaveSlotExists (string slotId) => SlotToData.ContainsKey(slotId);
        public override bool AnySaveExists () => SlotToData.Count > 0;

        public override void DeleteSaveSlot (string slotId)
        {
            if (!SaveSlotExists(slotId)) return;
            InvokeOnBeforeDelete(slotId);
            SlotToData.Remove(slotId);
            InvokeOnDeleted(slotId);
        }

        public override void RenameSaveSlot (string sourceSlotId, string destSlotId)
        {
            if (!SaveSlotExists(sourceSlotId)) return;
            InvokeOnBeforeRename(sourceSlotId, destSlotId);
            Save(destSlotId, Load(sourceSlotId));
            DeleteSaveSlot(sourceSlotId);
            InvokeOnRenamed(sourceSlotId, destSlotId);
        }
    }
}
