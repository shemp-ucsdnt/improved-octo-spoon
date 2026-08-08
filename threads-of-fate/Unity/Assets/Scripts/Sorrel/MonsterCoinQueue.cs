using System;
using System.Collections.Generic;

namespace ThreadsOfFate.Sorrel
{
	[Serializable]
	public struct MonsterCoin
	{
		public string formId;
	}

	// Strict 5-slot FIFO for collected Monster Coins: a 6th coin drop
	// automatically evicts the oldest entry.
	public class MonsterCoinQueue
	{
		public const int Capacity = 5;

		private readonly Queue<MonsterCoin> _coins = new Queue<MonsterCoin>();

		public event Action OnQueueChanged;
		public IReadOnlyCollection<MonsterCoin> Coins => _coins;

		public void Collect(string formId)
		{
			if (_coins.Count >= Capacity)
			{
				_coins.Dequeue();
			}

			_coins.Enqueue(new MonsterCoin { formId = formId });
			OnQueueChanged?.Invoke();
		}

		// Doesn't consume the coin — while it's in the queue, Sorrel can
		// transform into that form as many times as wanted. It only leaves
		// once a 6th distinct pickup evicts it.
		public bool Contains(string formId)
		{
			foreach (MonsterCoin coin in _coins)
			{
				if (coin.formId == formId)
				{
					return true;
				}
			}

			return false;
		}
	}
}
