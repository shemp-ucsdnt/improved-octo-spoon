#pragma once

#include "CoreMinimal.h"
#include "Engine/DataTable.h"
#include "ThreadTypes.h"
#include "ConstructTypes.generated.h"

UENUM(BlueprintType)
enum class EConstructState : uint8
{
	Inactive,
	Active,
	Deconstructing
};

// Data-table row: one row per deployable construct (Phlogiston Boiler, Tidal
// Conduit, Mist Weaver, Umbral Decoy, Prism Turret, ...).
USTRUCT(BlueprintType)
struct FConstructDefinition : public FTableRowBase
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Identity")
	FName ConstructId;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Identity")
	FText DisplayName;

	// Threads consumed from the Thread Inventory on deployment.
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Recipe")
	TArray<FThreadCost> RequiredThreads;

	// Continuous maintenance drain while deployed, e.g. Phlogiston Boiler
	// 3 MP/s, Tidal Conduit 2 MP/s, Mist Weaver 2 MP/s.
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Maintenance")
	float MaintenanceMPPerSecond = 0.f;

	// Flat costs for the Overclock high-impact attack (e.g. Supercritical
	// Blast, Maelstrom Surge, Flash Freeze).
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Overclock")
	float OverclockMPCost = 0.f;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Overclock")
	float OverclockKineticEnergyCost = 0.f;

	// Resolved by the combat system to a GameplayAbility/effect class.
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Overclock")
	FName OverclockAbilityId;

	// Whether Deconstruct() refunds RequiredThreads back to the inventory.
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Deconstruct")
	bool bRefundThreadsOnDeconstruct = true;

	// Utility blast triggered on manual deconstruct (e.g. Steam Screen,
	// Aquatic Cleansing, Dew Drop). Left unset for constructs with no
	// deconstruct utility effect.
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Deconstruct")
	FName DeconstructUtilityId;
};

// Per-instance runtime state for a deployed construct. Stores a copy of the
// definition rather than a pointer so it survives data-table reloads and is
// safe to replicate/reflect.
USTRUCT(BlueprintType)
struct FActiveConstruct
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadOnly, Category = "Runtime")
	FConstructDefinition Definition;

	UPROPERTY(BlueprintReadOnly, Category = "Runtime")
	EConstructState State = EConstructState::Inactive;

	UPROPERTY(BlueprintReadOnly, Category = "Runtime")
	TWeakObjectPtr<AActor> SpawnedActor;
};
