using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default lazy enlightenment.
        /// </summary>
        public sealed class LazyEnlightenment : ILazyEnlightenment
        {
            ILazy<T> ILazyEnlightenment.CreateLazy<T>(Func<T> factory)
            {
                return new Lazy<T>(factory);
            }

            /// <summary>
            /// Simple (and somewhat inefficient) lazy initialization. Note that this implementation calls <c>factory</c> while under lock.
            /// </summary>
            /// <typeparam name="T">The type being lazy initialized.</typeparam>
            private sealed class Lazy<T> : ILazy<T>
            {
                /// <summary>
                /// The factory used to create the value.
                /// </summary>
                private readonly Func<T> _factory;

                /// <summary>
                /// Synchronization object.
                /// </summary>
                private readonly object _sync;

                /// <summary>
                /// The error encountered running the factory.
                /// </summary>
                private Exception _exception;

                /// <summary>
                /// The value.
                /// </summary>
                private T _value;

                /// <summary>
                /// A value indicating whether the factory method has run to completion.
                /// </summary>
                private bool _factoryCompleted;

                public Lazy(Func<T> factory)
                {
                    _factory = factory;
                    _sync = new object();
                }

                bool ILazy<T>.IsValueCreated
                {
                    get
                    {
                        lock (_sync)
                        {
                            return _factoryCompleted;
                        }
                    }
                }

                T ILazy<T>.Value
                {
                    get
                    {
                        lock (_sync)
                        {
                            if (!_factoryCompleted)
                            {
                                try
                                {
                                    _value = _factory();
                                }
                                catch (Exception ex)
                                {
                                    _exception = ex;
                                }

                                _factoryCompleted = true;
                            }

                            if (_exception != null)
                                throw Enlightenment.Exception.PrepareForRethrow(_exception);
                            return _value;
                        }
                    }
                }
            }
        }
    }
}
