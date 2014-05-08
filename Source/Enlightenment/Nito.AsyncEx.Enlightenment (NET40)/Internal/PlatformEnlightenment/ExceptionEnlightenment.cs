using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    // Use <c>ExceptionDispatchInfo</c> if possible, falling back on <c>Exception.PrepForRemoting</c>, with a final fallback on <see cref="Exception.Data"/>.
    public static class ExceptionEnlightenment
    {
        /// <summary>
        /// A delegate that will call <c>ExceptionDispatchInfo.Capture</c> followed by <c>ExceptionDispatchInfo.Throw</c>, or <c>null</c> if the <c>ExceptionDispatchInfo</c> type does not exist.
        /// </summary>
        private static readonly Action<Exception> CaptureAndThrow;

        /// <summary>
        /// A delegate that will call <c>Exception.PrepForRemoting</c>, or <c>null</c> if the method does not exist. This member is always <c>null</c> if <see cref="CaptureAndThrow"/> is non-<c>null</c>.
        /// </summary>
        private static readonly Action<Exception> PrepForRemoting;

        static ExceptionEnlightenment()
        {
            IReflectionExpressionProvider r = new ReflectionExpressionProvider();
            var exception = Expression.Parameter(typeof(Exception), "exception");
            var capture = r.Call(r.Type("System.Runtime.ExceptionServices.ExceptionDispatchInfo"), "Capture", exception);
            var @throw = r.Call(capture, "Throw");
            CaptureAndThrow = r.Compile<Action<Exception>>(@throw, exception);

            var prepForRemoting = r.Call(exception, "PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic);
            PrepForRemoting = r.Compile<Action<Exception>>(prepForRemoting, exception);
        }

        public static Exception PrepareForRethrow(Exception exception)
        {
            if (CaptureAndThrow != null)
            {
                CaptureAndThrow(exception);
            }
            else if (PrepForRemoting != null)
            {
                PrepForRemoting(exception);
            }
            else
            {
                TryAddStackTrace(exception);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to add the original stack trace to the <see cref="Exception.Data"/> collection.
        /// </summary>
        /// <param name="exception">The exception. May not be <c>null</c>.</param>
        private static void TryAddStackTrace(Exception exception)
        {
            try
            {
                exception.Data.Add("Original stack trace", exception.StackTrace);
            }
            catch (ArgumentException)
            {
                // Vexing exception
            }
            catch (NotSupportedException)
            {
                // Vexing exception
            }
        }
    }
}
