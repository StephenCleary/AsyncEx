using System;
using System.Threading;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A <see cref="Token"/> that may or may not also reference its own <see cref="CancellationTokenSource"/>. Instances of this type should always be disposed.
    /// </summary>
    public sealed class NormalizedCancellationToken : IDisposable
    {
        /// <summary>
        /// The <see cref="CancellationTokenSource"/>, if any. If this is not <c>null</c>, then <see cref="_token"/> is <c>_cts.Token</c>.
        /// </summary>
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// The <see cref="Token"/>. If <see cref="_cts"/> is not <c>null</c>, then this is <c>_cts.Token</c>.
        /// </summary>
        private readonly CancellationToken _token;

        /// <summary>
        /// Creates a normalized cancellation token that can never be canceled.
        /// </summary>
        public NormalizedCancellationToken()
        {
        }

        /// <summary>
        /// Creates a normalized cancellation token from a <see cref="CancellationTokenSource"/>. <see cref="Token"/> is set to the <see cref="CancellationTokenSource.Token"/> property of <paramref name="cts"/>.
        /// </summary>
        /// <param name="cts">The source for this token.</param>
        public NormalizedCancellationToken(CancellationTokenSource cts)
        {
            _cts = cts;
            _token = cts.Token;
        }

        /// <summary>
        /// Creates a normalized cancellation token from a <see cref="CancellationToken"/>. <see cref="Token"/> is set to <paramref name="token"/>.
        /// </summary>
        /// <param name="token">The source for this token.</param>
        public NormalizedCancellationToken(CancellationToken token)
        {
            _token = token;
        }

        /// <summary>
        /// Releases any resources used by this normalized cancellation token.
        /// </summary>
        public void Dispose()
        {
            if (_cts != null)
                _cts.Dispose();
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> for this normalized cancellation token.
        /// </summary>
        public CancellationToken Token { get { return _token; } }
    }
}