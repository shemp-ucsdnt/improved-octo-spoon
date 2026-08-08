namespace ThreadsOfFate.Core
{
	// Minimal status-flag contract — enough for Chervil's status-cleansing
	// fog to have something real to clear. Deliberately not a full status
	// system (no durations, stacking, or tick damage): that's a separate
	// feature nothing in the spec has asked for yet.
	public interface IStatusAffectable
	{
		void ApplyStatus(string statusId);
		void ClearStatus(string statusId);
		void ClearAllStatuses();
		bool HasStatus(string statusId);
	}
}
