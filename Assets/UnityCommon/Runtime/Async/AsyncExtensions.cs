using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class AsyncExtensions
{
    public static TaskAwaiter<AsyncOperation> GetAwaiter (this AsyncOperation asyncOperation)
    {
        var taskCompletionSource = new TaskCompletionSource<AsyncOperation>();
        if (asyncOperation.isDone) taskCompletionSource.SetResult(asyncOperation);
        else asyncOperation.completed += op => taskCompletionSource.SetResult(op);
        return taskCompletionSource.Task.GetAwaiter();
    }

    public static TaskAwaiter<UnityWebRequestAsyncOperation> GetAwaiter (this UnityWebRequestAsyncOperation webRequestOperation)
    {
        var taskCompletionSource = new TaskCompletionSource<UnityWebRequestAsyncOperation>();
        if (webRequestOperation.isDone) taskCompletionSource.SetResult(webRequestOperation);
        else webRequestOperation.completed += op => taskCompletionSource.SetResult(op as UnityWebRequestAsyncOperation);
        return taskCompletionSource.Task.GetAwaiter();
    }

    public static TaskAwaiter<ResourceRequest> GetAwaiter (this ResourceRequest resourceRequest)
    {
        var taskCompletionSource = new TaskCompletionSource<ResourceRequest>();
        if (resourceRequest.isDone) taskCompletionSource.SetResult(resourceRequest);
        else resourceRequest.completed += op => taskCompletionSource.SetResult(op as ResourceRequest);
        return taskCompletionSource.Task.GetAwaiter();
    }

    public static TaskAwaiter<AssetBundleCreateRequest> GetAwaiter (this AssetBundleCreateRequest createBundleRequest)
    {
        var taskCompletionSource = new TaskCompletionSource<AssetBundleCreateRequest>();
        if (createBundleRequest.isDone) taskCompletionSource.SetResult(createBundleRequest);
        else createBundleRequest.completed += op => taskCompletionSource.SetResult(op as AssetBundleCreateRequest);
        return taskCompletionSource.Task.GetAwaiter();
    }

    public static TaskAwaiter<AssetBundleRequest> GetAwaiter (this AssetBundleRequest bundleRequest)
    {
        var taskCompletionSource = new TaskCompletionSource<AssetBundleRequest>();
        if (bundleRequest.isDone) taskCompletionSource.SetResult(bundleRequest);
        else bundleRequest.completed += op => taskCompletionSource.SetResult(op as AssetBundleRequest);
        return taskCompletionSource.Task.GetAwaiter();
    }

    /// <summary>
    /// Allows yielding async methods inside <see cref="Coroutine"/>.
    /// </summary>
    public static IEnumerator AsIEnumerator (this Task task)
    {
        while (!task.IsCompleted) yield return null;
        if (task.IsFaulted) ExceptionDispatchInfo.Capture(task.Exception).Throw();
    }

    /// <summary>
    /// Allows yielding async methods inside <see cref="Coroutine"/>.
    /// </summary>
    public static IEnumerator<T> AsIEnumerator<T> (this Task<T> task)
    {
        while (!task.IsCompleted) yield return default(T);
        if (task.IsFaulted) ExceptionDispatchInfo.Capture(task.Exception).Throw();
        yield return task.Result;
    }

    /// <summary>
    /// Allows to properly execute async methods from sync methods without waiting for <see cref="Task"/>.
    /// Required to receive exceptions from the underlying async method (otherwise it will fail silently).
    /// </summary>
    public static async void WrapAsync (this Task task) => await task;
}
