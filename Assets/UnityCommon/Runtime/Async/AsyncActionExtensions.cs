
public static class AsyncActionExtensions
{
    public static AsyncAction<T> CompleteInstantly<T> (this AsyncAction<T> action, T result)
    {
        action.CompleteInstantly(result);
        return action;
    }
    
}
