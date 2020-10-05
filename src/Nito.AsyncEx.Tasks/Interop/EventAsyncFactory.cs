using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Interop
{
    /// <summary>
    /// Creation methods for tasks wrapping events.
    /// </summary>
    public static class EventAsyncFactory
    {
        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of any type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event delegate.</typeparam>
        /// <typeparam name="TEventArguments">A type containing all event arguments.</typeparam>
        /// <param name="convert">A conversion delegate that takes an <see cref="Action{TEventArguments}"/> and converts it to a <typeparamref name="TDelegate"/>. This is generally of the form <c>x => (...) => x(new TEventArguments(...))</c>.</param>
        /// <param name="subscribe">A method that takes a <typeparamref name="TDelegate"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <typeparamref name="TDelegate"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static async Task<TEventArguments> FromAnyEvent<TDelegate, TEventArguments>(Func<Action<TEventArguments>, TDelegate> convert,
            Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        {
            _ = convert ?? throw new ArgumentNullException(nameof(convert));
            _ = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            _ = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));

            cancellationToken.ThrowIfCancellationRequested();
            var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<TEventArguments>();
            var subscription = convert(result => tcs.TrySetResult(result));
            try
            {
                using (cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
                {
                    subscribe(subscription);
                    return await tcs.Task.ConfigureAwait(continueOnCapturedContext: unsubscribeOnCapturedContext);
                }
            }
            finally
            {
                unsubscribe(subscription);
            }
        }

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of any type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event delegate.</typeparam>
        /// <typeparam name="TEventArguments">A type containing all event arguments.</typeparam>
        /// <param name="convert">A conversion delegate that takes an <see cref="Action{TEventArguments}"/> and converts it to a <typeparamref name="TDelegate"/>. This is generally of the form <c>x => (...) => x(new TEventArguments(...))</c>.</param>
        /// <param name="subscribe">A method that takes a <typeparamref name="TDelegate"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <typeparamref name="TDelegate"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<TEventArguments> FromAnyEvent<TDelegate, TEventArguments>(Func<Action<TEventArguments>, TDelegate> convert,
            Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken)
        => FromAnyEvent(convert, subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of any type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event delegate.</typeparam>
        /// <typeparam name="TEventArguments">A type containing all event arguments.</typeparam>
        /// <param name="convert">A conversion delegate that takes an <see cref="Action{TEventArguments}"/> and converts it to a <typeparamref name="TDelegate"/>. This is generally of the form <c>x => (...) => x(new TEventArguments(...))</c>.</param>
        /// <param name="subscribe">A method that takes a <typeparamref name="TDelegate"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <typeparamref name="TDelegate"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<TEventArguments> FromAnyEvent<TDelegate, TEventArguments>(Func<Action<TEventArguments>, TDelegate> convert,
            Action<TDelegate> subscribe, Action<TDelegate> unsubscribe)
        => FromAnyEvent(convert, subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="subscribe">A method that takes a <see cref="EventHandler"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="EventHandler"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, EventArgs>> FromEvent(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<EventHandler, EventArguments<object, EventArgs>>(x => (sender, args) => x(CreateEventArguments(sender, args)), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="subscribe">A method that takes a <see cref="EventHandler"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <see cref="EventHandler"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, EventArgs>> FromEvent(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, CancellationToken cancellationToken)
        => FromEvent(subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="subscribe">A method that takes a <see cref="EventHandler"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <see cref="EventHandler"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, EventArgs>> FromEvent(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe)
        => FromEvent(subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="subscribe">A method that takes a <see cref="EventHandler{TEventArgs}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="EventHandler{TEventArgs}"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, TEventArgs>> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<EventHandler<TEventArgs>, EventArguments<object, TEventArgs>>(x => (sender, args) => x(CreateEventArguments(sender, args)), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="subscribe">A method that takes a <see cref="EventHandler{TEventArgs}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <see cref="EventHandler{TEventArgs}"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, TEventArgs>> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe, CancellationToken cancellationToken)
        => FromEvent(subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="subscribe">A method that takes a <see cref="EventHandler{TEventArgs}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <see cref="EventHandler{TEventArgs}"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, TEventArgs>> FromEvent<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe)
        => FromEvent(subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that follow the standard <c>sender, eventArgs</c> pattern but with a custom delegate type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event delegate.</typeparam>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="convert">A conversion delegate that takes an <see cref="EventHandler{TEventArgs}"/> and converts it to a <typeparamref name="TDelegate"/>. If the type parameters are specified explicitly, this should be <c>x => x.Invoke</c>. If the type parameters are inferred, this should be <c>(EventHandler&lt;TEventArgs&gt; x) => new TDelegate(x)</c> with appropriate substitutions for <typeparamref name="TEventArgs"/> and <typeparamref name="TDelegate"/>.</param>
        /// <param name="subscribe">A method that takes a <typeparamref name="TDelegate"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <typeparamref name="TDelegate"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, TEventArgs>> FromEvent<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> convert, Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<TDelegate, EventArguments<object, TEventArgs>>(x => convert((sender, args) => x(CreateEventArguments(sender, args))), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that follow the standard <c>sender, eventArgs</c> pattern but with a custom delegate type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event delegate.</typeparam>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="convert">A conversion delegate that takes an <see cref="EventHandler{TEventArgs}"/> and converts it to a <typeparamref name="TDelegate"/>. If the type parameters are specified explicitly, this should be <c>x => x.Invoke</c>. If the type parameters are inferred, this should be <c>(EventHandler&lt;TEventArgs&gt; x) => new TDelegate(x)</c> with appropriate substitutions for <typeparamref name="TEventArgs"/> and <typeparamref name="TDelegate"/>.</param>
        /// <param name="subscribe">A method that takes a <typeparamref name="TDelegate"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <typeparamref name="TDelegate"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, TEventArgs>> FromEvent<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> convert, Action<TDelegate> subscribe, Action<TDelegate> unsubscribe, CancellationToken cancellationToken)
        => FromEvent(convert, subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that follow the standard <c>sender, eventArgs</c> pattern but with a custom delegate type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the event delegate.</typeparam>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="convert">A conversion delegate that takes an <see cref="EventHandler{TEventArgs}"/> and converts it to a <typeparamref name="TDelegate"/>. If the type parameters are specified explicitly, this should be <c>x => x.Invoke</c>. If the type parameters are inferred, this should be <c>(EventHandler&lt;TEventArgs&gt; x) => new TDelegate(x)</c> with appropriate substitutions for <typeparamref name="TEventArgs"/> and <typeparamref name="TDelegate"/>.</param>
        /// <param name="subscribe">A method that takes a <typeparamref name="TDelegate"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes a <typeparamref name="TDelegate"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<object, TEventArgs>> FromEvent<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> convert, Action<TDelegate> subscribe, Action<TDelegate> unsubscribe)
        => FromEvent(convert, subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action{TSender, TEventArgs}"/>.
        /// </summary>
        /// <typeparam name="TSender">The type of the "sender" (the first event argument).</typeparam>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="subscribe">A method that takes an <see cref="Action{TSender, TEventArgs}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action{TSender, TEventArgs}"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<TSender, TEventArgs>> FromActionEvent<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> subscribe, Action<Action<TSender, TEventArgs>> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<Action<TSender, TEventArgs>, EventArguments<TSender, TEventArgs>>(x => (sender, args) => x(CreateEventArguments(sender, args)), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action{TSender, TEventArgs}"/>.
        /// </summary>
        /// <typeparam name="TSender">The type of the "sender" (the first event argument).</typeparam>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="subscribe">A method that takes an <see cref="Action{TSender, TEventArgs}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action{TSender, TEventArgs}"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<TSender, TEventArgs>> FromActionEvent<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> subscribe, Action<Action<TSender, TEventArgs>> unsubscribe, CancellationToken cancellationToken)
        => FromActionEvent(subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action{TSender, TEventArgs}"/>.
        /// </summary>
        /// <typeparam name="TSender">The type of the "sender" (the first event argument).</typeparam>
        /// <typeparam name="TEventArgs">The type of the "arguments" (the second event argument).</typeparam>
        /// <param name="subscribe">A method that takes an <see cref="Action{TSender, TEventArgs}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action{TSender, TEventArgs}"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<EventArguments<TSender, TEventArgs>> FromActionEvent<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> subscribe, Action<Action<TSender, TEventArgs>> unsubscribe)
        => FromActionEvent(subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action{T}"/>.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the argument passed to the event handler and used to complete the task.</typeparam>
        /// <param name="subscribe">A method that takes an <see cref="Action{T}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action{T}"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<TEventArgs> FromActionEvent<TEventArgs>(Action<Action<TEventArgs>> subscribe, Action<Action<TEventArgs>> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<Action<TEventArgs>, TEventArgs>(x => x, subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action{T}"/>.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the argument passed to the event handler and used to complete the task.</typeparam>
        /// <param name="subscribe">A method that takes an <see cref="Action{T}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action{T}"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<TEventArgs> FromActionEvent<TEventArgs>(Action<Action<TEventArgs>> subscribe, Action<Action<TEventArgs>> unsubscribe, CancellationToken cancellationToken)
        => FromActionEvent(subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task{T}"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action{T}"/>.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the argument passed to the event handler and used to complete the task.</typeparam>
        /// <param name="subscribe">A method that takes an <see cref="Action{T}"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action{T}"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task<TEventArgs> FromActionEvent<TEventArgs>(Action<Action<TEventArgs>> subscribe, Action<Action<TEventArgs>> unsubscribe)
        => FromActionEvent(subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Returns a <see cref="Task"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action"/>.
        /// </summary>
        /// <param name="subscribe">A method that takes an <see cref="Action"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action"/> and unsubscribes it from the event. This method is invoked in a captured context if <paramref name="unsubscribeOnCapturedContext"/> is <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <param name="unsubscribeOnCapturedContext">Whether to invoke <paramref name="unsubscribe"/> on a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task FromActionEvent(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken, bool unsubscribeOnCapturedContext)
        => FromAnyEvent<Action, object?>(x => () => x(null), subscribe, unsubscribe, cancellationToken, unsubscribeOnCapturedContext);

        /// <summary>
        /// Returns a <see cref="Task"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action"/>.
        /// </summary>
        /// <param name="subscribe">A method that takes an <see cref="Action"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the task (and unsubscribe from the event handler).</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task FromActionEvent(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken)
            => FromActionEvent(subscribe, unsubscribe, cancellationToken, true);

        /// <summary>
        /// Returns a <see cref="Task"/> that completes when a specified event next fires. This overload is for events that are of type <see cref="Action"/>.
        /// </summary>
        /// <param name="subscribe">A method that takes an <see cref="Action"/> and subscribes it to the event.</param>
        /// <param name="unsubscribe">A method that takes an <see cref="Action"/> and unsubscribes it from the event. This method is always invoked in a captured context.</param>
        /// <remarks>
        /// <para>Calling this method in a loop is often an anti-pattern, because the event is only subscribed to when this method is invoked, and is unsubscribed from when the task completes. From the time the task is completed until this method is called again, the event may fire and be "lost". If you find yourself needing a loop around this method, consider using Rx or TPL Dataflow instead.</para>
        /// </remarks>
        public static Task FromActionEvent(Action<Action> subscribe, Action<Action> unsubscribe)
            => FromActionEvent(subscribe, unsubscribe, CancellationToken.None, true);

        /// <summary>
        /// Creates an <see cref="EventArguments{TSender,TEventArgs}"/> structure.
        /// </summary>
        /// <typeparam name="TSender">The type of the sender of the event.</typeparam>
        /// <typeparam name="TEventArgs">The event arguments.</typeparam>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static EventArguments<TSender, TEventArgs> CreateEventArguments<TSender, TEventArgs>(TSender sender, TEventArgs eventArgs)
        => new EventArguments<TSender, TEventArgs> { Sender = sender, EventArgs = eventArgs };
    }
}

