using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Nito.AsyncEx.Internals
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class ImmutableDeque<T> : IEnumerable<T>
	{
		/// <summary>
		/// 
		/// </summary>
#pragma warning disable CA1000
		public static ImmutableDeque<T> Empty { get; } = new(Node0.Instance, ImmutableList<T>.Empty, Node0.Instance);
#pragma warning restore CA1000

		/// <summary>
		/// 
		/// </summary>
		public bool IsEmpty => _left.IsEmpty && _mid.IsEmpty && _right.IsEmpty;

		public int Count => _left.Count + _mid.Count + _right.Count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public ImmutableDeque<T> EnqueueFront(T value)
		{
			var newLeft = _left.TryEnqueueFront(value);
			if (newLeft != null)
				return new(newLeft, _mid, _right);

			return new(new Node1(value), _mid.InsertRange(0, _left), _right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public ImmutableDeque<T> EnqueueBack(T value)
		{
			var newRight = _right.TryEnqueueBack(value);
			if (newRight != null)
				return new(_left, _mid, newRight);

			return new(_left, _mid.InsertRange(_mid.Count, _right), new Node1(value));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public ImmutableDeque<T> DequeueFront(out T value)
		{
			{
				var newLeft = _left.TryDequeueFront(out value);
				if (newLeft != null)
					return new(newLeft, _mid, _right);
			}

			{
				var newLeft = TrySpliceFromMid(out var newMid, out value);
				if (newLeft != null)
					return new(newLeft, newMid, _right);
			}

			{
				var newRight = _right.TryDequeueFront(out value);
				if (newRight != null)
					return new(_left, _mid, newRight);
			}

			throw new InvalidOperationException("Deque is empty");

			INode? TrySpliceFromMid(out ImmutableList<T> newMid, out T value)
			{
				Span<T> temp = new T[4];
				int count = 0;
				foreach (var item in _mid)
				{
					temp[count] = item;
					++count;
					if (count == 4)
						break;
				}

				if (count == 0)
				{
					newMid = default!;
					value = default!;
					return null;
				}

				value = temp[0];
				newMid = count == 4 ? _mid.RemoveRange(0, 4) : ImmutableList<T>.Empty;
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
				return count switch
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
				{
					1 => Node0.Instance,
					2 => new Node1(temp[1]),
					3 => new Node2(temp[1], temp[2]),
					4 => new Node3(temp[1], temp[2], temp[3]),
				};
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public ImmutableDeque<T> DequeueBack(out T value)
		{
			{
				var newRight = _right.TryDequeueBack(out value);
				if (newRight != null)
					return new(_left, _mid, newRight);
			}

			{
				var newRight = TrySpliceFromMid(out var newMid, out value);
				if (newRight != null)
					return new(_left, newMid, newRight);
			}

			{
				var newLeft = _left.TryDequeueBack(out value);
				if (newLeft != null)
					return new(newLeft, _mid, _right);
			}

			throw new InvalidOperationException("Deque is empty");

			INode? TrySpliceFromMid(out ImmutableList<T> newMid, out T value)
			{
				var count = Math.Min(_mid.Count, 4);
				var splice = count == _mid.Count ? _mid : _mid.GetRange(_mid.Count - 4, 4);

				Span<T> temp = splice.ToArray();

				if (count == 0)
				{
					newMid = default!;
					value = default!;
					return null;
				}

				value = temp[count - 1];
				newMid = count == 4 ? _mid.RemoveRange(_mid.Count - 4, 4) : ImmutableList<T>.Empty;
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
				return count switch
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
				{
					1 => Node0.Instance,
					2 => new Node1(temp[0]),
					3 => new Node2(temp[0], temp[1]),
					4 => new Node3(temp[0], temp[1], temp[2]),
				};
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public ImmutableDeque<T>? RemoveOne(Func<T, bool> predicate, out T value)
		{
			{
				var newLeft = _left.RemoveOne(predicate, out value);
				if (newLeft != null)
					return new(newLeft, _mid, _right);
			}
			{
				var newRight = _right.RemoveOne(predicate, out value);
				if (newRight != null)
					return new(_left, _mid, newRight);
			}
			{
				var index = _mid.FindIndex(x => predicate(x));
				if (index != -1)
				{
					value = _mid[index];
					return new(_left, _mid.RemoveAt(index), _right);
				}
			}

			value = default!;
			return null;
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach (var item in _left)
				yield return item;
			foreach (var item in _mid)
				yield return item;
			foreach (var item in _right)
				yield return item;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private ImmutableDeque(INode left, ImmutableList<T> mid, INode right)
		{
			_left = left;
			_mid = mid;
			_right = right;
		}

		private readonly INode _left;
		private readonly ImmutableList<T> _mid;
		private readonly INode _right;

		private interface INode : IEnumerable<T>
		{
			bool IsEmpty { get; }
			int Count { get; }
			INode? TryEnqueueFront(T value);
			INode? TryEnqueueBack(T value);
			INode? TryDequeueFront(out T value);
			INode? TryDequeueBack(out T value);
			INode? RemoveOne(Func<T, bool> predicate, out T value);
		}

		private sealed class Node0 : INode
		{
			public static Node0 Instance { get; } = new();

			public bool IsEmpty => true;
			public int Count => 0;
			public INode? TryEnqueueFront(T value) => new Node1(value);
			public INode? TryEnqueueBack(T value) => new Node1(value);
			public INode? TryDequeueFront(out T value)
			{
				value = default!;
				return null;
			}
			public INode? TryDequeueBack(out T value)
			{
				value = default!;
				return null;
			}
			public INode? RemoveOne(Func<T, bool> predicate, out T value)
			{
				value = default!;
				return null;
			}

			public IEnumerator<T> GetEnumerator()
			{
				yield break;
			}
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private sealed class Node1 : INode
		{
			public Node1(T value1)
			{
				Value1 = value1;
			}

			private T Value1 { get; }

			public bool IsEmpty => false;
			public int Count => 1;
			public INode? TryEnqueueFront(T value) => new Node2(value, Value1);
			public INode? TryEnqueueBack(T value) => new Node2(Value1, value);
			public INode? TryDequeueFront(out T value)
			{
				value = Value1;
				return Node0.Instance;
			}
			public INode? TryDequeueBack(out T value)
			{
				value = Value1;
				return Node0.Instance;
			}
			public INode? RemoveOne(Func<T, bool> predicate, out T value)
			{
				var test = predicate(Value1);
				value = test ? Value1 : default!;
				return test ? Node0.Instance : null;
			}

			public IEnumerator<T> GetEnumerator()
			{
				yield return Value1;
			}
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
		private sealed class Node2 : INode
		{
			public Node2(T value1, T value2)
			{
				Value1 = value1;
				Value2 = value2;
			}

			private T Value1 { get; }
			private T Value2 { get; }

			public bool IsEmpty => false;
			public int Count => 2;
			public INode? TryEnqueueFront(T value) => new Node3(value, Value1, Value2);
			public INode? TryEnqueueBack(T value) => new Node3(Value1, Value2, value);
			public INode? TryDequeueFront(out T value)
			{
				value = Value1;
				return new Node1(Value2);
			}
			public INode? TryDequeueBack(out T value)
			{
				value = Value2;
				return new Node1(Value1);
			}
			public INode? RemoveOne(Func<T, bool> predicate, out T value)
			{
				{
					var test = predicate(Value1);
					if (test)
					{
						value = Value1;
						return new Node1(Value2);
					}
				}
				{
					var test = predicate(Value2);
					if (test)
					{
						value = Value2;
						return new Node1(Value1);
					}
				}
				value = default!;
				return null;
			}

			public IEnumerator<T> GetEnumerator()
			{
				yield return Value1;
				yield return Value2;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
		private sealed class Node3 : INode
		{
			public Node3(T value1, T value2, T value3)
			{
				Value1 = value1;
				Value2 = value2;
				Value3 = value3;
			}

			private T Value1 { get; }
			private T Value2 { get; }
			private T Value3 { get; }

			public bool IsEmpty => false;
			public int Count => 3;
			public INode? TryEnqueueFront(T value) => new Node4(value, Value1, Value2, Value3);
			public INode? TryEnqueueBack(T value) => new Node4(Value1, Value2, Value3, value);
			public INode? TryDequeueFront(out T value)
			{
				value = Value1;
				return new Node2(Value2, Value3);
			}
			public INode? TryDequeueBack(out T value)
			{
				value = Value3;
				return new Node2(Value1, Value2);
			}
			public INode? RemoveOne(Func<T, bool> predicate, out T value)
			{
				{
					var test = predicate(Value1);
					if (test)
					{
						value = Value1;
						return new Node2(Value2, Value3);
					}
				}
				{
					var test = predicate(Value2);
					if (test)
					{
						value = Value2;
						return new Node2(Value1, Value3);
					}
				}
				{
					var test = predicate(Value3);
					if (test)
					{
						value = Value3;
						return new Node2(Value1, Value2);
					}
				}
				value = default!;
				return null;
			}

			public IEnumerator<T> GetEnumerator()
			{
				yield return Value1;
				yield return Value2;
				yield return Value3;
			}
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private sealed class Node4 : INode
		{
			public Node4(T value1, T value2, T value3, T value4)
			{
				Value1 = value1;
				Value2 = value2;
				Value3 = value3;
				Value4 = value4;
			}

			private T Value1 { get; }
			private T Value2 { get; }
			private T Value3 { get; }
			private T Value4 { get; }

			public bool IsEmpty => false;
			public int Count => 4;
			public INode? TryEnqueueFront(T value) => null;
			public INode? TryEnqueueBack(T value) => null;
			public INode? TryDequeueFront(out T value)
			{
				value = Value1;
				return new Node3(Value2, Value3, Value4);
			}
			public INode? TryDequeueBack(out T value)
			{
				value = Value4;
				return new Node3(Value1, Value2, Value3);
			}
			public INode? RemoveOne(Func<T, bool> predicate, out T value)
			{
				{
					var test = predicate(Value1);
					if (test)
					{
						value = Value1;
						return new Node3(Value2, Value3, Value4);
					}
				}
				{
					var test = predicate(Value2);
					if (test)
					{
						value = Value2;
						return new Node3(Value1, Value3, Value4);
					}
				}
				{
					var test = predicate(Value3);
					if (test)
					{
						value = Value3;
						return new Node3(Value1, Value2, Value4);
					}
				}
				{
					var test = predicate(Value4);
					if (test)
					{
						value = Value4;
						return new Node3(Value1, Value2, Value3);
					}
				}
				value = default!;
				return null;
			}

			public IEnumerator<T> GetEnumerator()
			{
				yield return Value1;
				yield return Value2;
				yield return Value3;
				yield return Value4;
			}
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
