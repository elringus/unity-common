
/// <summary>
/// Allows switching to the main Unity thread in async methods.
/// Use this to return back to the main thread after using <see cref="WaitForBackgroundThread"/>.
/// </summary>
public class WaitForUnityThread : CoroutineRunner
{
    public override bool CanBeInstantlyCompleted => false;
}
