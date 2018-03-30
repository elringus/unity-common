using System;
using System.Collections.Generic;

public static class AsyncActionExtensions
{
    public static AsyncAction<T> CompleteInstantly<T> (this AsyncAction<T> action, T result)
    {
        action.CompleteInstantly(result);
        return action;
    }

    /// <summary>
    /// Applies provided func over each list element (in order), chaining async actions execution.
    /// </summary>
    public static AsyncAction InvokeAsyncList<T> (this IList<T> asyncList, Func<T, AsyncAction> invokeFunc)
    {
        var queue = new Queue<T>(asyncList);
        var lastAction = AsyncAction.CreateCompleted();

        for (int i = 0; i < asyncList.Count; i++)
            lastAction = lastAction.ThenAsync(() => invokeFunc(queue.Dequeue()));

        return lastAction;
    }
}
