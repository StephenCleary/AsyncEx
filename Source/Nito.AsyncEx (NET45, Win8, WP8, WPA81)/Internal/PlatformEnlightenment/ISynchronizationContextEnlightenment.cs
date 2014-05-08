using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// Provides a way to set <see cref="SynchronizationContext.Current"/>.
    /// </summary>
    public interface ISynchronizationContextEnlightenment
    {
        /// <summary>
        /// Sets <see cref="SynchronizationContext.Current"/>
        /// </summary>
        /// <param name="context">The context to set.</param>
        void SetCurrent(SynchronizationContext context);
    }
}
