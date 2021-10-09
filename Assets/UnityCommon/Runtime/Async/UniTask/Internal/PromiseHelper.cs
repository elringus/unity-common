using System.Collections.Generic;

namespace UnityCommon.Async.Internal
{
    internal static class PromiseHelper
    {
        internal static void TrySetResultAll<TPromise, T> (IEnumerable<TPromise> source, T value)
            where TPromise : class, IResolvePromise<T>
        {
            var rentArray = ArrayPoolUtil.Materialize(source);
            var clearArray = true;
            try
            {
                var array = rentArray.Array;
                var len = rentArray.Length;
                for (int i = 0; i < len; i++)
                {
                    array[i].TrySetResult(value);
                    array[i] = null;
                }
                clearArray = false;
            }
            finally
            {
                rentArray.DisposeManually(clearArray);
            }
        }
    }
}
