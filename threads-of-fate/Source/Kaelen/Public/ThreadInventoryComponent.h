#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "ThreadTypes.h"
#include "ThreadInventoryComponent.generated.h"

DECLARE_MULTICAST_DELEGATE(FOnThreadInventoryChanged);

// Owns Kaelen's elemental/material thread counts (Beast, Iron, Fire, Wind,
// Lightning, Water, Phantom, Crystal). Construct deployment spends threads
// via ConsumeThreads(); Deconstruct() may return them via RefundThreads().
UCLASS(ClassGroup = (Kaelen), meta = (BlueprintSpawnableComponent))
class UThreadInventoryComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category = "Kaelen|Threads")
	void AddThreads(EThreadElement Element, int32 Count);

	UFUNCTION(BlueprintPure, Category = "Kaelen|Threads")
	int32 GetThreadCount(EThreadElement Element) const;

	UFUNCTION(BlueprintPure, Category = "Kaelen|Threads")
	bool HasThreads(const TArray<FThreadCost>& Cost) const;

	// All-or-nothing: returns false and leaves the inventory untouched if any
	// single element is short.
	UFUNCTION(BlueprintCallable, Category = "Kaelen|Threads")
	bool ConsumeThreads(const TArray<FThreadCost>& Cost);

	UFUNCTION(BlueprintCallable, Category = "Kaelen|Threads")
	void RefundThreads(const TArray<FThreadCost>& Cost);

	// Observer hook for UI (thread counters) and construct-recipe availability checks.
	FOnThreadInventoryChanged OnThreadInventoryChanged;

private:
	UPROPERTY()
	TMap<EThreadElement, int32> Threads;
};
