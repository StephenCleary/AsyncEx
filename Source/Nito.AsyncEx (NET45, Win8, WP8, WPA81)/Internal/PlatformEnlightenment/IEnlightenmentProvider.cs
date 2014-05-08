namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// An enlightenment provider, which creates enlightenments on demand.
    /// </summary>
    public interface IEnlightenmentProvider
    {
        /// <summary>
        /// Creates an enlightenment of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of enlightenment to create.</typeparam>
        T CreateEnlightenment<T>() where T : class;
    }
}
