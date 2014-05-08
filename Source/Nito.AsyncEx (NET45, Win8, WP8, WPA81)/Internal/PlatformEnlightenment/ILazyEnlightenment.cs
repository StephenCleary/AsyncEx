using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// A lazy-created value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface ILazy<out T>
    {
        /// <summary>
        /// Whether or not the value is created.
        /// </summary>
        bool IsValueCreated { get; }

        /// <summary>
        /// Gets the value, creating it if necessary.
        /// </summary>
        T Value { get; }
    }
    
    /// <summary>
    /// Provides lazy-created values.
    /// </summary>
    public interface ILazyEnlightenment
    {
        /// <summary>
        /// Creates a new lazy-created value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="factory">The factory method used to create the value on-demand. This method may be called while under lock.</param>
        ILazy<T> CreateLazy<T>(Func<T> factory);
    }
}
