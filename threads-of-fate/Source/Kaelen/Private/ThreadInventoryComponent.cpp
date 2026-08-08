#include "ThreadInventoryComponent.h"

void UThreadInventoryComponent::AddThreads(EThreadElement Element, int32 Count)
{
	if (Count <= 0)
	{
		return;
	}

	Threads.FindOrAdd(Element) += Count;
	OnThreadInventoryChanged.Broadcast();
}

int32 UThreadInventoryComponent::GetThreadCount(EThreadElement Element) const
{
	const int32* Found = Threads.Find(Element);
	return Found ? *Found : 0;
}

bool UThreadInventoryComponent::HasThreads(const TArray<FThreadCost>& Cost) const
{
	for (const FThreadCost& Entry : Cost)
	{
		if (GetThreadCount(Entry.Element) < Entry.Count)
		{
			return false;
		}
	}
	return true;
}

bool UThreadInventoryComponent::ConsumeThreads(const TArray<FThreadCost>& Cost)
{
	if (!HasThreads(Cost))
	{
		return false;
	}

	for (const FThreadCost& Entry : Cost)
	{
		Threads[Entry.Element] -= Entry.Count;
	}

	OnThreadInventoryChanged.Broadcast();
	return true;
}

void UThreadInventoryComponent::RefundThreads(const TArray<FThreadCost>& Cost)
{
	for (const FThreadCost& Entry : Cost)
	{
		AddThreads(Entry.Element, Entry.Count);
	}
}
