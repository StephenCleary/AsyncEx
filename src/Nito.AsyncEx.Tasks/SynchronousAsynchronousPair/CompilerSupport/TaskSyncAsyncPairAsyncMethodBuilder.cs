using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nito.AsyncEx.SynchronousAsynchronousPair.CompilerSupport
{
    /// <summary>
    /// Compiler support for returning <see cref="ISynchronousAsynchronousTaskPair{T}"/> from <c>async</c> methods.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    public struct SyncAsyncTaskPairAsyncMethodBuilder<T>
    {
        private readonly AsyncTaskMethodBuilder<T> _taskMethodBuilder;
        private readonly LinkedSynchronousAsynchronousTaskPair<T> _link;

        /// <summary>
        /// Compiler support for creating an async method builder struct.
        /// </summary>
        /// <param name="taskMethodBuilder">The task method builder used to control the resulting task pair.</param>
        public SyncAsyncTaskPairAsyncMethodBuilder(AsyncTaskMethodBuilder<T> taskMethodBuilder)
        {
            _link = new LinkedSynchronousAsynchronousTaskPair<T>(taskMethodBuilder.Task);
        }

        /// <summary>
        /// Compiler support for creating an async method builder struct.
        /// </summary>
        /// <returns>The async method builder struct.</returns>
        public static SyncAsyncTaskPairAsyncMethodBuilder<T> Create() => new SyncAsyncTaskPairAsyncMethodBuilder<T>(AsyncTaskMethodBuilder<T>.Create());

        /// <summary>
        /// Compiler support. Starts the state machine.
        /// </summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine.</param>
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
            _taskMethodBuilder.Start(ref stateMachine);

        /// <summary>
        /// Compiler support. Does nothing.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _taskMethodBuilder.SetStateMachine(stateMachine);

        /// <summary>
        /// Compiler support. Notification that the state machine has successfully completed.
        /// </summary>
        /// <param name="result">The result.</param>
        public void SetResult(T result) => _taskMethodBuilder.SetResult(result);

        /// <summary>
        /// Compiler support. Notification that the state machine has faulted.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void SetException(Exception exception) => _taskMethodBuilder.SetException(exception);

        /// <summary>
        /// Compiler support. Registers a continuation with the awaiter that continues the state machine.
        /// </summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

        /// <summary>
        /// Compiler support. Registers a continuation with the awaiter that continues the state machine.
        /// </summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _taskMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        /// <summary>
        /// Compiler support. The instance returned from the <c>async</c> method.
        /// </summary>
        public ISynchronousAsynchronousTaskPair<T> Task => _link;
    }
}
