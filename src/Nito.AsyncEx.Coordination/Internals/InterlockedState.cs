using System;
using System.Threading;

namespace Nito.AsyncEx.Internals
{
	/// <summary>
	/// Interlocked helper methods.
	/// </summary>
	internal static class InterlockedState
	{
		/// <summary>
		/// Executes a state transition from one state to another.
		/// </summary>
		/// <typeparam name="T">The type of the state; this is generally an immutable type. Strongly consider using a record class.</typeparam>
		/// <param name="state">The location of the state.</param>
		/// <param name="transformation">The transformation to apply to the state. This may be invoked any number of times and should be a pure function.</param>
		/// <param name="oldState">The old value of the state.</param>
		/// <returns>The old state and the new state.</returns>
		public static T Transform<T>(ref T state, Func<T, T> transformation, out T oldState)
			where T : class?
		{
			_ = transformation ?? throw new ArgumentNullException(nameof(transformation));

			while (true)
			{
				oldState = Interlocked.CompareExchange(ref state, default!, default!);
				var newState = transformation(oldState);
				if (ReferenceEquals(Interlocked.CompareExchange(ref state!, newState, oldState), oldState))
					return newState;
			}
		}

		/// <summary>
		/// Executes a state transition from one state to another.
		/// </summary>
		/// <typeparam name="T">The type of the state; this is generally an immutable type. Strongly consider using a record class.</typeparam>
		/// <param name="state">The location of the state.</param>
		/// <param name="transformation">The transformation to apply to the state. This may be invoked any number of times and should be a pure function.</param>
		/// <returns>The old state and the new state.</returns>
		public static T Transform<T>(ref T state, Func<T, T> transformation)
			where T : class?
		{
			_ = transformation ?? throw new ArgumentNullException(nameof(transformation));

			while (true)
			{
				var oldState = Interlocked.CompareExchange(ref state, default!, default!);
				var newState = transformation(oldState);
				if (ReferenceEquals(Interlocked.CompareExchange(ref state!, newState, oldState), oldState))
					return newState;
			}
		}

		/// <summary>
		/// Reads the current state. Note that the state may have changed by the time this method returns.
		/// </summary>
		/// <typeparam name="T">The type of the state; this is generally an immutable type. Strongly consider using a record class.</typeparam>
		/// <param name="state">The location of the state.</param>
		/// <returns>The current state.</returns>
		public static T Read<T>(ref T state)
			where T : class? =>
			Interlocked.CompareExchange(ref state, default!, default!);
	}
}
