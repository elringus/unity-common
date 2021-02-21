using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityCommon
{
    public class ResourcesHolder
    {
        private readonly Dictionary<string, List<WeakReference>> pathToHolders = new Dictionary<string, List<WeakReference>>();
        private readonly Action<string> unloadAction;

        public ResourcesHolder (Action<string> unloadAction)
        {
            this.unloadAction = unloadAction;
        }

        public void Hold (string path, object holder)
        {
            GetHoldersFor(path).Add(new WeakReference(holder));
        }

        public void Release (string path, object holder, bool unload = true)
        {
            var holders = GetHoldersFor(path);
            Release(path, holders, holder, unload);
        }

        public void ReleaseAll (object holder, bool unload = true)
        {
            foreach (var kv in pathToHolders)
                if (IsHeldBy(kv.Value, holder))
                    Release(kv.Key, kv.Value, holder, unload);
        }

        public bool IsHeldBy (string path, object holder) => IsHeldBy(GetHoldersFor(path), holder);

        public int CountHolders (string path) => CountHolders(GetHoldersFor(path));

        private List<WeakReference> GetHoldersFor (string path)
        {
            if (pathToHolders.TryGetValue(path, out var holders)) return holders;
            holders = new List<WeakReference>();
            pathToHolders[path] = holders;
            return holders;
        }

        private int CountHolders (List<WeakReference> holders)
        {
            return holders.Count(wr => wr.IsAlive);
        }

        private void Release (string path, List<WeakReference> holders, object holder, bool unload)
        {
            holders.RemoveAll(wr => !wr.IsAlive || wr.Target == holder);
            if (unload && CountHolders(holders) == 0)
                unloadAction(path);
        }

        private bool IsHeldBy (List<WeakReference> holders, object holder)
        {
            return holders.Any(wr => wr.IsAlive && wr.Target == holder);
        }
    }
}
