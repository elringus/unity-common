using System;
using System.Collections.Generic;

namespace UnityCommon
{
    /// <summary>
    /// Wrapper over <see cref="System.Threading.CancellationToken"/> allowing two cancellation scenarios:
    /// ASAP, requiring the async routine to cancel (return) as soon as possible and lazy, allowing to perform completion procedures beforehand.
    /// </summary>
    public readonly struct CancellationToken : IEquatable<CancellationToken>
    {
        /// <summary>
        /// Whether cancellation of the async routine is requested (either ASAP or lazy).
        /// </summary>
        public bool CancellationRequested => asapToken.IsCancellationRequested || lazyToken.IsCancellationRequested;
        /// <inheritdoc cref="CancellationRequested"/>
        public bool IsCancellationRequested => CancellationRequested;
        /// <summary>
        /// Whether cancellation of the async routine is requested as soon as possible.
        /// </summary>
        public bool CancelASAP => asapToken.IsCancellationRequested;
        /// <summary>
        /// Whether cancellation of the async routine is requested, but it's allowed to perform completion procedures beforehand.
        /// </summary>
        public bool CancelLazy => !CancelASAP && lazyToken.IsCancellationRequested;

        private readonly System.Threading.CancellationToken asapToken, lazyToken;

        /// <param name="cancellationToken">Source cancellation token.</param>
        /// <param name="lazy">Whether the provided token allows lazy cancellation.</param>
        public CancellationToken (System.Threading.CancellationToken cancellationToken, bool lazy = false)
        {
            if (lazy) lazyToken = cancellationToken;
            else asapToken = cancellationToken;
        }

        /// <param name="asapToken">Source token for ASAP cancellation scenario.</param>
        /// <param name="lazyToken">Source token for lazy cancellation scenario.</param>
        public CancellationToken (System.Threading.CancellationToken asapToken, System.Threading.CancellationToken lazyToken)
        {
            this.asapToken = asapToken;
            this.lazyToken = lazyToken;
        }

        /// <summary>
        /// Returns <see cref="System.Threading.CancellationTokenSource"/> of the combined ASAP and lazy tokens.
        /// Don't forget to dispose the returned CTS to prevent memory leaks.
        /// </summary>
        public System.Threading.CancellationTokenSource CreateLinkedTokenSource ()
        {
            return System.Threading.CancellationTokenSource.CreateLinkedTokenSource(asapToken, lazyToken);
        }

        public static bool operator == (CancellationToken left, CancellationToken right) => left.Equals(right);

        public static bool operator != (CancellationToken left, CancellationToken right) => !(left == right);

        public override bool Equals (object obj)
        {
            return obj is CancellationToken token && Equals(token);
        }

        public bool Equals (CancellationToken other)
        {
            return EqualityComparer<System.Threading.CancellationToken>.Default.Equals(asapToken, other.asapToken) &&
                   EqualityComparer<System.Threading.CancellationToken>.Default.Equals(lazyToken, other.lazyToken);
        }

        public override int GetHashCode ()
        {
            int hashCode = -1518707389;
            hashCode = hashCode * -1521134295 + asapToken.GetHashCode();
            hashCode = hashCode * -1521134295 + lazyToken.GetHashCode();
            return hashCode;
        }
    }
}
