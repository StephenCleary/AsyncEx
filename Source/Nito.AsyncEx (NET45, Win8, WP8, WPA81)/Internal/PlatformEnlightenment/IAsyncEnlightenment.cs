using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// A portable interface to async-related utilities.
    /// </summary>
    public interface IAsyncEnlightenment
    {
        /// <summary>
        /// Adds the <c>DenyChildAttach</c> bit to the existing <see cref="TaskCreationOptions"/>.
        /// </summary>
        /// <param name="options">The existing <see cref="TaskCreationOptions"/></param>
        TaskCreationOptions AddDenyChildAttach(TaskCreationOptions options);
    }
}
