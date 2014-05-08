using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.ComponentModel;

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
    public class TaskCompletionNotifierUnitTests
    {
        [Test]
        public void Notifier_TaskCompletesSuccessfully_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource();
            var notifier = NotifyTaskCompletion.Create(tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);

            tcs.SetResult();

            Assert.IsFalse(taskNotification());
            Assert.IsTrue(statusNotification());
            Assert.IsTrue(isCompletedNotification());
            Assert.IsTrue(isSuccessfullyCompletedNotification());
            Assert.IsFalse(isCanceledNotification());
            Assert.IsFalse(isFaultedNotification());
            Assert.IsFalse(exceptionNotification());
            Assert.IsFalse(innerExceptionNotification());
            Assert.IsFalse(errorMessageNotification());
        }

        [Test]
        public void Notifier_TaskCanceled_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource();
            var notifier = NotifyTaskCompletion.Create(tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);

            tcs.SetCanceled();

            Assert.IsFalse(taskNotification());
            Assert.IsTrue(statusNotification());
            Assert.IsTrue(isCompletedNotification());
            Assert.IsFalse(isSuccessfullyCompletedNotification());
            Assert.IsTrue(isCanceledNotification());
            Assert.IsFalse(isFaultedNotification());
            Assert.IsFalse(exceptionNotification());
            Assert.IsFalse(innerExceptionNotification());
            Assert.IsFalse(errorMessageNotification());
        }

        [Test]
        public void Notifier_TaskFaulted_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource();
            var notifier = NotifyTaskCompletion.Create(tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);

            tcs.SetException(new NotImplementedException());

            Assert.IsFalse(taskNotification());
            Assert.IsTrue(statusNotification());
            Assert.IsTrue(isCompletedNotification());
            Assert.IsFalse(isSuccessfullyCompletedNotification());
            Assert.IsFalse(isCanceledNotification());
            Assert.IsTrue(isFaultedNotification());
            Assert.IsTrue(exceptionNotification());
            Assert.IsTrue(innerExceptionNotification());
            Assert.IsTrue(errorMessageNotification());
        }

        [Test]
        public void NotifierT_TaskCompletesSuccessfully_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTaskCompletion.Create(tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            tcs.SetResult(null);

            Assert.IsFalse(taskNotification());
            Assert.IsTrue(statusNotification());
            Assert.IsTrue(isCompletedNotification());
            Assert.IsTrue(isSuccessfullyCompletedNotification());
            Assert.IsFalse(isCanceledNotification());
            Assert.IsFalse(isFaultedNotification());
            Assert.IsFalse(exceptionNotification());
            Assert.IsFalse(innerExceptionNotification());
            Assert.IsFalse(errorMessageNotification());
            Assert.IsTrue(resultNotification());
        }

        [Test]
        public void NotifierT_TaskCanceled_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTaskCompletion.Create(tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            tcs.SetCanceled();

            Assert.IsFalse(taskNotification());
            Assert.IsTrue(statusNotification());
            Assert.IsTrue(isCompletedNotification());
            Assert.IsFalse(isSuccessfullyCompletedNotification());
            Assert.IsTrue(isCanceledNotification());
            Assert.IsFalse(isFaultedNotification());
            Assert.IsFalse(exceptionNotification());
            Assert.IsFalse(innerExceptionNotification());
            Assert.IsFalse(errorMessageNotification());
            Assert.IsFalse(resultNotification());
        }

        [Test]
        public void NotifierT_TaskFaulted_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTaskCompletion.Create(tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            tcs.SetException(new NotImplementedException());

            Assert.IsFalse(taskNotification());
            Assert.IsTrue(statusNotification());
            Assert.IsTrue(isCompletedNotification());
            Assert.IsFalse(isSuccessfullyCompletedNotification());
            Assert.IsFalse(isCanceledNotification());
            Assert.IsTrue(isFaultedNotification());
            Assert.IsTrue(exceptionNotification());
            Assert.IsTrue(innerExceptionNotification());
            Assert.IsTrue(errorMessageNotification());
            Assert.IsFalse(resultNotification());
        }

        private static Func<bool> PropertyNotified<T, P>(T obj, Expression<Func<T, P>> prop) where T : INotifyPropertyChanged
        {
            var expression = (MemberExpression)prop.Body;
            string name = expression.Member.Name;

            bool invoked = false;
            obj.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == name)
                    invoked = true;
            };

            return () => invoked;
        }
    }
}
