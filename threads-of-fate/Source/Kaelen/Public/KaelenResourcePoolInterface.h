#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "KaelenResourcePoolInterface.generated.h"

// Decouples the Construct Deployment Manager from a specific character class:
// anything that owns MP and Kinetic Energy (generated via whip-dagger melee
// combos) can implement this to be drained/spent by Kaelen's constructs.
UINTERFACE(BlueprintType)
class UKaelenResourcePool : public UInterface
{
	GENERATED_BODY()
};

class IKaelenResourcePool
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintNativeEvent, Category = "Kaelen|Resources")
	float GetCurrentMP() const;

	// Consumes up to Amount, clamped at 0 (so continuous drain can run a pool
	// dry rather than being rejected outright). Returns false if the pool
	// held less than Amount. Callers that need all-or-nothing semantics
	// (e.g. Overclock's flat cost) should check GetCurrentMP() first.
	UFUNCTION(BlueprintNativeEvent, Category = "Kaelen|Resources")
	bool ConsumeMP(float Amount);

	UFUNCTION(BlueprintNativeEvent, Category = "Kaelen|Resources")
	float GetKineticEnergy() const;

	UFUNCTION(BlueprintNativeEvent, Category = "Kaelen|Resources")
	bool ConsumeKineticEnergy(float Amount);
};
