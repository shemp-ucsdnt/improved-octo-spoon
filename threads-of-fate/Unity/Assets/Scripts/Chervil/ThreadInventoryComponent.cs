using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreadsOfFate.Chervil
{
	// Owns Chervil's elemental/material thread counts (Beast, Iron, Fire, Wind,
	// Lightning, Water, Phantom, Crystal). Construct deployment spends threads
	// via ConsumeThreads(); Deconstruct() may return them via RefundThreads().
	public class ThreadInventoryComponent : MonoBehaviour
	{
		// Observer hook for UI (thread counters) and recipe-availability checks.
		public event Action OnThreadInventoryChanged;

		private readonly Dictionary<ThreadElement, int> _threads = new Dictionary<ThreadElement, int>();

		public void AddThreads(ThreadElement element, int count)
		{
			if (count <= 0)
			{
				return;
			}

			_threads.TryGetValue(element, out int current);
			_threads[element] = current + count;
			OnThreadInventoryChanged?.Invoke();
		}

		public int GetThreadCount(ThreadElement element)
		{
			return _threads.TryGetValue(element, out int count) ? count : 0;
		}

		public bool HasThreads(IReadOnlyList<ThreadCost> cost)
		{
			foreach (ThreadCost entry in cost)
			{
				if (GetThreadCount(entry.element) < entry.count)
				{
					return false;
				}
			}
			return true;
		}

		// All-or-nothing: returns false and leaves the inventory untouched if
		// any single element is short.
		public bool ConsumeThreads(IReadOnlyList<ThreadCost> cost)
		{
			if (!HasThreads(cost))
			{
				return false;
			}

			foreach (ThreadCost entry in cost)
			{
				_threads[entry.element] -= entry.count;
			}

			OnThreadInventoryChanged?.Invoke();
			return true;
		}

		public void RefundThreads(IReadOnlyList<ThreadCost> cost)
		{
			foreach (ThreadCost entry in cost)
			{
				AddThreads(entry.element, entry.count);
			}
		}
	}
}
