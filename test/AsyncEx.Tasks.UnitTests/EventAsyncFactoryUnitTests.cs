using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Interop;
using Xunit;

namespace UnitTests
{
    public class EventAsyncFactoryUnitTests
    {
        [Fact]
        public void CompilationTest_CommonScenarios()
        {
            var target = new EventfulType();

            // EventHandler
            Assert.Same(Typeof<Task<EventArguments<object, EventArgs>>>(), Typeof(
                EventAsyncFactory.FromEvent(h => target.NongenericEventHandlerEvent += h, h => target.NongenericEventHandlerEvent -= h)
            ));

            // EventHandler<T>
            Assert.Same(Typeof<Task<EventArguments<object, int>>>(), Typeof(
                EventAsyncFactory.FromEvent<int>(h => target.GenericEventHandlerEvent += h, h => target.GenericEventHandlerEvent -= h)
            ));

            // Custom delegate type of the pattern `void TDelegate(object sender, TEventArgs eventArgs)`
            // E.g., PropertyChangedEventHandler (with PropertyChangedEventArgs)
            Assert.Same(Typeof<Task<EventArguments<object, PropertyChangedEventArgs>>>(), Typeof(
                EventAsyncFactory.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(d => d.Invoke, h => target.PropertyChangedEventHandlerEvent += h, h => target.PropertyChangedEventHandlerEvent -= h)
            ));
            // Alternatively:
            Assert.Same(Typeof<Task<EventArguments<object, PropertyChangedEventArgs>>>(), Typeof(
                EventAsyncFactory.FromEvent((EventHandler<PropertyChangedEventArgs> d) => new PropertyChangedEventHandler(d), h => target.PropertyChangedEventHandlerEvent += h, h => target.PropertyChangedEventHandlerEvent -= h)
            ));
        }

        [Fact]
        public void CompilationTest_LessCommonScenarios()
        {
            var target = new EventfulType();

            // Action
            Assert.Same(Typeof<Task>(), Typeof(
                EventAsyncFactory.FromActionEvent(h => target.ActionEvent += h, h => target.ActionEvent -= h)
            ));

            // Action<T>
            Assert.Same(Typeof<Task<int>>(), Typeof(
                EventAsyncFactory.FromActionEvent<int>(h => target.ActionIntEvent += h, h => target.ActionIntEvent -= h)
            ));

            // Action<TSender, T>
            Assert.Same(Typeof<Task<EventArguments<EventfulType, int>>>(), Typeof(
                EventAsyncFactory.FromActionEvent<EventfulType, int>(h => target.ActionSenderIntEvent += h, h => target.ActionSenderIntEvent -= h)
            ));
        }

        [Fact]
        public void CompilationTest_RareScenarios()
        {
            var target = new EventfulType();

            // Custom delegate type (creating the "result type" inline - in this case a simple tuple)
            Assert.Same(Typeof<Task<Tuple<int, int, int, int>>>(), Typeof(
                EventAsyncFactory.FromAnyEvent<Action<int, int, int, int>, Tuple<int, int, int, int>>(h => (a, b, c, d) => h(Tuple.Create(a, b, c, d)), h => target.CustomEvent += h, h => target.CustomEvent -= h)
            ));

            // Custom delegate type with return value
            Assert.Same(Typeof<Task<Tuple<int, int, int>>>(), Typeof(
                EventAsyncFactory.FromAnyEvent<Func<int, int, int, int>, Tuple<int, int, int>>(h => (a, b, c) =>
                {
                    h(Tuple.Create(a, b, c));
                    return 13;
                }, h => target.CustomReturnEvent += h, h => target.CustomReturnEvent -= h)
            ));
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_NongenericEventHandler()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromEvent(h => target.NongenericEventHandlerEvent += h, h => target.NongenericEventHandlerEvent -= h);
            target.OnNongenericEventHandlerEvent();
            var result = await task;
            Assert.Same(target, result.Sender);
            Assert.Equal(EventArgs.Empty, result.EventArgs);
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_GenericEventHandler()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromEvent<int>(h => target.GenericEventHandlerEvent += h, h => target.GenericEventHandlerEvent -= h);
            target.OnGenericEventHandlerEvent(13);
            var result = await task;
            Assert.Same(target, result.Sender);
            Assert.Equal(13, result.EventArgs);
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_PropertyChangedEventHandler()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(d => d.Invoke, h => target.PropertyChangedEventHandlerEvent += h, h => target.PropertyChangedEventHandlerEvent -= h);
            target.OnPropertyChangedEventHandlerEvent(new PropertyChangedEventArgs("bob"));
            var result = await task;
            Assert.Same(target, result.Sender);
            Assert.Equal("bob", result.EventArgs.PropertyName);
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_Action()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromActionEvent(h => target.ActionEvent += h, h => target.ActionEvent -= h);
            target.OnActionEvent();
            await task;
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_ActionInt()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromActionEvent<int>(h => target.ActionIntEvent += h, h => target.ActionIntEvent -= h);
            target.OnActionIntEvent(17);
            var result = await task;
            Assert.Equal(17, result);
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_ActionSenderInt()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromActionEvent<EventfulType, int>(h => target.ActionSenderIntEvent += h, h => target.ActionSenderIntEvent -= h);
            target.OnActionSenderIntEvent(11);
            var result = await task;
            Assert.Same(target, result.Sender);
            Assert.Equal(11, result.EventArgs);
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_Custom()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromAnyEvent<Action<int, int, int, int>, Tuple<int, int, int, int>>(h => (a, b, c, d) => h(Tuple.Create(a, b, c, d)), h => target.CustomEvent += h, h => target.CustomEvent -= h);
            target.OnCustomEvent(3, 5, 7, 23);
            var result = await task;
            Assert.Equal(3, result.Item1);
            Assert.Equal(5, result.Item2);
            Assert.Equal(7, result.Item3);
            Assert.Equal(23, result.Item4);
        }

        [Fact]
        public async Task EventRaised_CompletesTaskWithData_CustomReturn()
        {
            var target = new EventfulType();
            var task = EventAsyncFactory.FromAnyEvent<Func<int, int, int, int>, Tuple<int, int, int>>(h => (a, b, c) =>
            {
                h(Tuple.Create(a, b, c));
                return 13;
            }, h => target.CustomReturnEvent += h, h => target.CustomReturnEvent -= h);
            var eventReturn = target.OnCustomReturnEvent(3, 7, 23);
            var result = await task;
            Assert.Equal(3, result.Item1);
            Assert.Equal(7, result.Item2);
            Assert.Equal(23, result.Item3);
            Assert.Equal(13, eventReturn);
        }

        private static Type Typeof<T>(T _ = default(T))
        {
            return typeof(T);
        }

        private sealed class EventfulType
        {
            public event EventHandler NongenericEventHandlerEvent;
            public event EventHandler<int> GenericEventHandlerEvent;
            public event PropertyChangedEventHandler PropertyChangedEventHandlerEvent;

            public event Action ActionEvent;
            public event Action<int> ActionIntEvent;
            public event Action<EventfulType, int> ActionSenderIntEvent;

            public event Action<int, int, int, int> CustomEvent;
            public event Func<int, int, int, int> CustomReturnEvent;

            public void OnNongenericEventHandlerEvent() => NongenericEventHandlerEvent?.Invoke(this, EventArgs.Empty);
            public void OnGenericEventHandlerEvent(int e) => GenericEventHandlerEvent?.Invoke(this, e);
            public void OnPropertyChangedEventHandlerEvent(PropertyChangedEventArgs e) => PropertyChangedEventHandlerEvent?.Invoke(this, e);
            public void OnActionEvent() => ActionEvent?.Invoke();
            public void OnActionIntEvent(int obj) => ActionIntEvent?.Invoke(obj);
            public void OnActionSenderIntEvent(int arg2) => ActionSenderIntEvent?.Invoke(this, arg2);
            public void OnCustomEvent(int arg1, int arg2, int arg3, int arg4) => CustomEvent?.Invoke(arg1, arg2, arg3, arg4);
            public int? OnCustomReturnEvent(int arg1, int arg2, int arg3) => CustomReturnEvent?.Invoke(arg1, arg2, arg3);
        }
    }
}
