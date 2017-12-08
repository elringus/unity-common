using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AsyncActionExtensions
{
    public static AsyncAction<T> CompleteInstantly<T> (this AsyncAction<T> action, T state)
    {
        action.CompleteInstantly(state);
        return action;
    }
    
}
