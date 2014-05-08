using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Tests
{
    [TestFixture]
    public class SynchronizationContextHelpersUnitTests_NET40
    {
        [Test]
        public void CurrentOrDefault_FromThreadPoolThread_ReturnsDefault()
        {
            AsyncContext.Run(async () =>
            {
                var result = await Task.Run(() => SynchronizationContextHelpers.CurrentOrDefault);
                Assert.AreEqual(typeof(SynchronizationContext), result.GetType());
            });
        }

        [Test]
        public void SynchronizationContextSwitcher_SetsCurrent()
        {
            var context = new SynchronizationContext();
            Assert.AreNotEqual(context, SynchronizationContext.Current);
            using (new SynchronizationContextHelpers.SynchronizationContextSwitcher(context))
            {
                Assert.AreEqual(context, SynchronizationContext.Current);
            }
            Assert.AreNotEqual(context, SynchronizationContext.Current);
        }

        [Test]
        public void CurrentOrDefault_FromCustomContext_ReturnsCurrent()
        {
            var context = new SynchronizationContext();
            using (new SynchronizationContextHelpers.SynchronizationContextSwitcher(context))
            {
                Assert.AreEqual(context, SynchronizationContextHelpers.CurrentOrDefault);
            }
        }

        [Test]
        public void SynchronizationContextSwitcher_SecondDispose_DoesNotReinstateContext()
        {
            var context1 = new SynchronizationContext();
            var context2 = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context1);
            Assert.AreSame(context1, SynchronizationContext.Current);
            var switcher = new SynchronizationContextHelpers.SynchronizationContextSwitcher(context2);
            Assert.AreSame(context2, SynchronizationContext.Current);
            ((IDisposable)switcher).Dispose();
            Assert.AreSame(context1, SynchronizationContext.Current);
            SynchronizationContext.SetSynchronizationContext(context2);
            Assert.AreSame(context2, SynchronizationContext.Current);
            ((IDisposable)switcher).Dispose();
            Assert.AreSame(context2, SynchronizationContext.Current);
        }
    }
}
