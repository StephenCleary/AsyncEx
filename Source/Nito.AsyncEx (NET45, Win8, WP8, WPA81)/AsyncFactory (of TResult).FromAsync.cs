using Nito.AsyncEx.Internal;
using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    public static partial class AsyncFactory<TResult>
    {
        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0>(Func<TArg0, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1>(Func<TArg0, TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2>(Func<TArg0, TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3>(Func<TArg0, TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <typeparam name="TArg8">The type of argument 8.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <typeparam name="TArg8">The type of argument 8.</typeparam>
        /// <typeparam name="TArg9">The type of argument 9.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <param name="arg9">Argument 9.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <typeparam name="TArg8">The type of argument 8.</typeparam>
        /// <typeparam name="TArg9">The type of argument 9.</typeparam>
        /// <typeparam name="TArg10">The type of argument 10.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <param name="arg9">Argument 9.</param>
        /// <param name="arg10">Argument 10.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <typeparam name="TArg8">The type of argument 8.</typeparam>
        /// <typeparam name="TArg9">The type of argument 9.</typeparam>
        /// <typeparam name="TArg10">The type of argument 10.</typeparam>
        /// <typeparam name="TArg11">The type of argument 11.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <param name="arg9">Argument 9.</param>
        /// <param name="arg10">Argument 10.</param>
        /// <param name="arg11">Argument 11.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <typeparam name="TArg8">The type of argument 8.</typeparam>
        /// <typeparam name="TArg9">The type of argument 9.</typeparam>
        /// <typeparam name="TArg10">The type of argument 10.</typeparam>
        /// <typeparam name="TArg11">The type of argument 11.</typeparam>
        /// <typeparam name="TArg12">The type of argument 12.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <param name="arg9">Argument 9.</param>
        /// <param name="arg10">Argument 10.</param>
        /// <param name="arg11">Argument 11.</param>
        /// <param name="arg12">Argument 12.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <typeparam name="TArg0">The type of argument 0.</typeparam>
        /// <typeparam name="TArg1">The type of argument 1.</typeparam>
        /// <typeparam name="TArg2">The type of argument 2.</typeparam>
        /// <typeparam name="TArg3">The type of argument 3.</typeparam>
        /// <typeparam name="TArg4">The type of argument 4.</typeparam>
        /// <typeparam name="TArg5">The type of argument 5.</typeparam>
        /// <typeparam name="TArg6">The type of argument 6.</typeparam>
        /// <typeparam name="TArg7">The type of argument 7.</typeparam>
        /// <typeparam name="TArg8">The type of argument 8.</typeparam>
        /// <typeparam name="TArg9">The type of argument 9.</typeparam>
        /// <typeparam name="TArg10">The type of argument 10.</typeparam>
        /// <typeparam name="TArg11">The type of argument 11.</typeparam>
        /// <typeparam name="TArg12">The type of argument 12.</typeparam>
        /// <typeparam name="TArg13">The type of argument 13.</typeparam>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="arg0">Argument 0.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <param name="arg9">Argument 9.</param>
        /// <param name="arg10">Argument 10.</param>
        /// <param name="arg11">Argument 11.</param>
        /// <param name="arg12">Argument 12.</param>
        /// <param name="arg13">Argument 13.</param>
        /// <returns>The result of the asynchronous operation.</returns>
        public static Task<TResult> FromApm<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>(Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            beginMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, Callback(endMethod, tcs), null);
            return tcs.Task;
        }

    }
}