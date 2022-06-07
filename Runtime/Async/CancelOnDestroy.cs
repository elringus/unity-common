using System.Threading;
using UnityEngine;

namespace UnityCommon
{
    public class CancelOnDestroy : MonoBehaviour
    {
        public CancellationToken Token => cts.Token;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private void OnDestroy ()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
