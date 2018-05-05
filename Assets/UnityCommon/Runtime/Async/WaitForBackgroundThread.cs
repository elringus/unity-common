using System.Runtime.CompilerServices;
using System.Threading.Tasks;

/// <summary>
/// Allows switching to background threads in async methods.
/// In Unity synchronization context is overwritten to always stay on the main thread;
/// using this we can force switch to the background thread. Use <see cref="WaitForUnityThread"/>
/// to switch back to the main thread if needed.
/// </summary>
public class WaitForBackgroundThread
{
    public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter ()
    {
        return Task.Run(() => { }).ConfigureAwait(false).GetAwaiter();
    }
}
