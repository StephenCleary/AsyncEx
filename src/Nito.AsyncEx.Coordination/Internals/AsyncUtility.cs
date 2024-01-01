using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internals;

internal static class AsyncUtility
{
	public static Task ForceAsync(Task task)
	{
		_ = task ?? throw new ArgumentNullException(nameof(task));
		return task.IsCompleted ? task : ForceAsyncCore(task);

		static async Task ForceAsyncCore(Task task)
		{
			await task.ConfigureAwait(false);
			await Task.Yield();
		}
	}

	public static Task<TResult> ForceAsync<TResult>(Task<TResult> task)
	{
		_ = task ?? throw new ArgumentNullException(nameof(task));
		return task.IsCompleted ? task : ForceAsyncCore(task);

		static async Task<TResult> ForceAsyncCore(Task<TResult> task)
		{
			var result = await task.ConfigureAwait(false);
			await Task.Yield();
			return result;
		}
	}
}