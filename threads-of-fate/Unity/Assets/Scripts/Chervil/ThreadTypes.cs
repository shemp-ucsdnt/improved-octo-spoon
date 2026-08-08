using System;

namespace ThreadsOfFate.Chervil
{
	public enum ThreadElement
	{
		Beast,
		Iron,
		Fire,
		Wind,
		Lightning,
		Water,
		Phantom,
		Crystal
	}

	[Serializable]
	public struct ThreadCost
	{
		public ThreadElement element;
		public int count;
	}
}
