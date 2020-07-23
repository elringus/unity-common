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
        /// Source token for ASAP cancellation scenario.
        /// </summary>
        public readonly System.Threading.CancellationToken ASAPToken;
        /// <summary>
        /// Source token for lazy cancellation scenario.
        /// </summary>
        public readonly System.Threading.CancellationToken LazyToken;
        /// <summary>
        /// Whether cancellation of the async routine is requested (either ASAP or lazy).
        /// </summary>
        public bool CancellationRequested => ASAPToken.IsCancellationRequested || LazyToken.IsCancellationRequested;
        /// <inheritdoc cref="CancellationRequested"/>
        public bool IsCancellationRequested => CancellationRequested;
        /// <summary>
        /// Whether cancellation of the async routine is requested as soon as possible.
        /// </summary>
        public bool CancelASAP => ASAPToken.IsCancellationRequested;
        /// <summary>
        /// Whether cancellation of the async routine is requested, but it's allowed to perform completion procedures beforehand.
        /// </summary>
        public bool CancelLazy => !CancelASAP && LazyToken.IsCancellationRequested;

        /// <param name="cancellationToken">Source cancellation token.</param>
        /// <param name="lazy">Whether the provided token allows lazy cancellation.</param>
        public CancellationToken (System.Threading.CancellationToken cancellationToken, bool lazy = false)
        {
            if (lazy) LazyToken = cancellationToken;
            else ASAPToken = cancellationToken;
        }

        /// <param name="asapToken">Source token for ASAP cancellation scenario.</param>
        /// <param name="lazyToken">Source token for lazy cancellation scenario.</param>
        public CancellationToken (System.Threading.CancellationToken asapToken, System.Threading.CancellationToken lazyToken)
        {
            ASAPToken = asapToken;
            LazyToken = lazyToken;
        }

        /// <summary>
        /// Returns <see cref="System.Threading.CancellationTokenSource"/> of the combined ASAP and lazy tokens.
        /// Don't forget to dispose the returned CTS to prevent memory leaks.
        /// </summary>
        public System.Threading.CancellationTokenSource CreateLinkedTokenSource ()
        {
            return System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ASAPToken, LazyToken);
        }

        public static implicit operator CancellationToken (System.Threading.CancellationToken token) => new CancellationToken(token);

        public static bool operator == (CancellationToken left, CancellationToken right) => left.Equals(right);

        public static bool operator != (CancellationToken left, CancellationToken right) => !(left == right);

        public override bool Equals (object obj)
        {
            return obj is CancellationToken token && Equals(token);
        }

        public bool Equals (CancellationToken other)
        {
            return EqualityComparer<System.Threading.CancellationToken>.Default.Equals(ASAPToken, other.ASAPToken) &&
                   EqualityComparer<System.Threading.CancellationToken>.Default.Equals(LazyToken, other.LazyToken);
        }

        public override int GetHashCode ()
        {
            int hashCode = -1518707389;
            hashCode = hashCode * -1521134295 + ASAPToken.GetHashCode();
            hashCode = hashCode * -1521134295 + LazyToken.GetHashCode();
            return hashCode;
        }
    }
}
