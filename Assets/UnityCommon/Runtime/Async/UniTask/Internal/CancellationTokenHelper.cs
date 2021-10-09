using System.Threading;

namespace UnityCommon.Async.Internal
{
    internal static class CancellationTokenHelper
    {
        public static bool TrySetOrLinkCancellationToken (ref CancellationToken field, CancellationToken newCancellationToken)
        {
            if (newCancellationToken == CancellationToken.None)
            {
                return false;
            }
            else if (field == CancellationToken.None)
            {
                field = newCancellationToken;
                return true;
            }
            else if (field == newCancellationToken)
            {
                return false;
            }

            field = CancellationTokenSource.CreateLinkedTokenSource(field, newCancellationToken).Token;
            return true;
        }
    }
}
