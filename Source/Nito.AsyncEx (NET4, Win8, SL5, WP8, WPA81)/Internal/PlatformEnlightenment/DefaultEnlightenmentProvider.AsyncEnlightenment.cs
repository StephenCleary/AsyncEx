using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default async enlightenment.
        /// </summary>
        public sealed class AsyncEnlightenment : IAsyncEnlightenment
        {
            /// <summary>
            /// The <c>TaskCreationOptions.DenyChildAttach</c> value, if it exists; otherwise, <c>0</c>.
            /// </summary>
            private readonly TaskCreationOptions _denyChildAttach;

            /// <summary>
            /// Looks up the <c>TaskCreationOptions.DenyChildAttach</c> value.
            /// </summary>
            public AsyncEnlightenment(IReflectionExpressionProvider r)
            {
                _denyChildAttach = r.EnumValue<TaskCreationOptions>("DenyChildAttach") ?? 0;
            }

            TaskCreationOptions IAsyncEnlightenment.AddDenyChildAttach(TaskCreationOptions options)
            {
                return options | _denyChildAttach;
            }
        }
    }
}
