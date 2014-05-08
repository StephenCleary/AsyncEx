using System;
using System.Threading;
using System.Threading.Tasks;

internal static class AndroidWorkarounds
{
	// Xamarin.Android has a bug in its Task.Delay.
	//   https://bugzilla.xamarin.com/show_bug.cgi?id=13318
	// Still in the tooling as of 2013-11-05.
	// TODO: I don't think it's that bug. Need to investigate further.
	public static Task Delay(int timeout)
	{
		var tcs = new TaskCompletionSource<object> ();
		new Timer (_ => tcs.TrySetResult (null)).Change (timeout, Timeout.Infinite);
		return tcs.Task;
	}
}