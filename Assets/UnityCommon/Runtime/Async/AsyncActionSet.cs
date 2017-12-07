using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a set of <see cref="AsyncAction"/>.
/// <see cref="OnCompleted"/> event will be envoked when all tracked actions complete.
/// </summary>
public class AsyncActionSet : AsyncAction
{
    public float Progress { get { return completedActionCount / actions.Count; } }
    public override bool CanBeInstantlyCompleted { get { return actions.All(a => a.CanBeInstantlyCompleted); } }

    private HashSet<AsyncAction> actions;
    private int completedActionCount;

    public AsyncActionSet (params AsyncAction[] asyncActions)
    {
        actions = new HashSet<AsyncAction>(asyncActions);
        foreach (var action in actions)
            action.Then(HandleOnCompleted);
    }

    public override void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;

        foreach (var action in actions)
            action.CompleteInstantly();
    }

    protected override void HandleOnCompleted ()
    {
        completedActionCount++;
        if (completedActionCount == actions.Count)
            base.HandleOnCompleted();
    }
}
