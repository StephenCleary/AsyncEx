using System;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using Nito.AsyncEx;
using System.Diagnostics.CodeAnalysis;

namespace Tests
{
    [TestFixture]
    public class ExceptionHelpersUnitTests_NET40
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNotImplementedException()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void PrepareForRethrow_PreservesStackTrace()
        {
            string originalStackTrace = null;
            try
            {
                Exception preservedException = null;
                try
                {
                    ThrowNotImplementedException();
                }
                catch (NotImplementedException ex)
                {
                    originalStackTrace = ex.StackTrace;
                    preservedException = ex;
                }

                throw ExceptionHelpers.PrepareForRethrow(preservedException);
            }
            catch (NotImplementedException ex)
            {
                Assert.IsTrue(ex.StackTrace.StartsWith(originalStackTrace));
                return;
            }
        }
    }
}
