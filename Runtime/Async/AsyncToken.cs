using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityCommon
{
    /// <summary>
    /// Controls execution flow of the async routines.
    /// </summary>
    /// <remarks>
    /// When the token is provided for an async method, implementation is expected to check for
    /// <see cref="Canceled"/> and <see cref="Completed"/> (when applicable) after each async activity and react accordingly.
    /// </remarks>
    public readonly struct AsyncToken : IEquatable<AsyncToken>
    {
        /// <summary>
        /// A token in a permanent cancellation state.
        /// </summary>
        public static AsyncToken CanceledToken { get; } = new AsyncToken(new CancellationToken(true));
        /// <summary>
        /// A token in a permanent completion state.
        /// </summary>
        public static AsyncToken CompletedToken { get; } = new AsyncToken(new CancellationToken(true), true);

        /// <summary>
        /// Source token for cancellation scenario.
        /// </summary>
        public readonly CancellationToken CancellationToken;
        /// <summary>
        /// Source token for completion scenario.
        /// </summary>
        public readonly CancellationToken CompletionToken;

        /// <summary>
        /// Whether cancellation of the async routine is requested as soon as possible.
        /// </summary>
        public bool Canceled => CancellationToken.IsCancellationRequested;
        /// <summary>
        /// Whether completion of the async routine is requested as soon as possible, but it's not canceled.
        /// </summary>
        public bool Completed => !Canceled && CompletionToken.IsCancellationRequested;
        /// <summary>
        /// Whether cancellation or completion of the async routine is requested.
        /// </summary>
        public bool CanceledOrCompleted => CancellationToken.IsCancellationRequested || CompletionToken.IsCancellationRequested;

        /// <param name="cancellationToken">Source cancellation token.</param>
        /// <param name="completion">Whether the provided token is for completion.</param>
        public AsyncToken (CancellationToken cancellationToken, bool completion = false)
        {
            if (completion) CompletionToken = cancellationToken;
            else CancellationToken = cancellationToken;
        }

        /// <param name="cancellationToken">Source token for cancellation scenario.</param>
        /// <param name="completionToken">Source token for completion scenario.</param>
        public AsyncToken (CancellationToken cancellationToken, CancellationToken completionToken)
        {
            CancellationToken = cancellationToken;
            CompletionToken = completionToken;
        }

        /// <summary>
        /// Returns <see cref="System.Threading.CancellationTokenSource"/> of the combined cancel and complete tokens.
        /// Don't forget to dispose the returned CTS to prevent memory leaks.
        /// </summary>
        public CancellationTokenSource CreateLinkedTokenSource ()
        {
            return CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, CompletionToken);
        }

        public static implicit operator AsyncToken (CancellationToken token) => new AsyncToken(token);

        public static bool operator == (AsyncToken left, AsyncToken right) => left.Equals(right);

        public static bool operator != (AsyncToken left, AsyncToken right) => !(left == right);

        public override bool Equals (object obj) => obj is AsyncToken token && Equals(token);

        public bool Equals (AsyncToken other)
        {
            return EqualityComparer<CancellationToken>.Default.Equals(CancellationToken, other.CancellationToken) &&
                   EqualityComparer<CancellationToken>.Default.Equals(CompletionToken, other.CompletionToken);
        }

        public override int GetHashCode ()
        {
            int hashCode = -1518707389;
            hashCode = hashCode * -1521134295 + CancellationToken.GetHashCode();
            hashCode = hashCode * -1521134295 + CompletionToken.GetHashCode();
            return hashCode;
        }
    }
}