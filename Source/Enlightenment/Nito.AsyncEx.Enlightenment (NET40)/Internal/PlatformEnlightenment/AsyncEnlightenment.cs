using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class AsyncEnlightenment
    {
        /// <summary>
        /// The <c>TaskCreationOptions.DenyChildAttach</c> value, if it exists; otherwise, <c>0</c>.
        /// </summary>
        private static readonly TaskCreationOptions DenyChildAttach;

        static AsyncEnlightenment()
        {
            DenyChildAttach = ReflectionHelper.EnumValue<TaskCreationOptions>("DenyChildAttach") ?? 0;
        }

        public static TaskCreationOptions AddDenyChildAttach(TaskCreationOptions options)
        {
            return options | DenyChildAttach;
        }
    }
}
