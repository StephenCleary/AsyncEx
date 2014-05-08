using System;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using Nito.AsyncEx;
using System.Diagnostics.CodeAnalysis;

#if NET40
#if NO_ENLIGHTENMENT
namespace Tests_NET4_NE
#else
namespace Tests_NET4
#endif
#else
#if NO_ENLIGHTENMENT
namespace Tests_NE
#else
namespace Tests
#endif
#endif
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class ExceptionHelpersUnitTests
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
