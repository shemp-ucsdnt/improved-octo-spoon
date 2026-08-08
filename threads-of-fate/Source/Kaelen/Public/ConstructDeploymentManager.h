#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "ConstructTypes.h"
#include "ConstructDeploymentManager.generated.h"

class UThreadInventoryComponent;
class UDataTable;
struct FConstructDefinition;

DECLARE_MULTICAST_DELEGATE_OneParam(FOnConstructDeployed, FName /*ConstructId*/);
DECLARE_MULTICAST_DELEGATE_TwoParams(FOnConstructDeconstructed, FName /*ConstructId*/, bool /*bForced*/);
DECLARE_MULTICAST_DELEGATE_TwoParams(FOnConstructOverclocked, FName /*ConstructId*/, FName /*OverclockAbilityId*/);

// Manages Kaelen's deployed constructs: continuous MP maintenance drain,
// Overclock (flat MP + Kinetic Energy for a high-impact attack), and
// Deconstruct (safe unsummon, optionally refunding threads / firing a
// utility blast). Auto-deconstructs everything if MP hits 0.
UCLASS(ClassGroup = (Kaelen), meta = (BlueprintSpawnableComponent))
class UConstructDeploymentManager : public UActorComponent
{
	GENERATED_BODY()

public:
	UConstructDeploymentManager();

	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	UFUNCTION(BlueprintCallable, Category = "Kaelen|Constructs")
	bool DeployConstruct(FName ConstructId);

	UFUNCTION(BlueprintCallable, Category = "Kaelen|Constructs")
	bool Overclock(FName ConstructId);

	UFUNCTION(BlueprintCallable, Category = "Kaelen|Constructs")
	void Deconstruct(FName ConstructId, bool bForced = false);

	UFUNCTION(BlueprintPure, Category = "Kaelen|Constructs")
	bool IsConstructActive(FName ConstructId) const;

	// Fired for UI/VFX/audio hooks (Observer pattern).
	FOnConstructDeployed OnConstructDeployed;
	FOnConstructDeconstructed OnConstructDeconstructed;
	FOnConstructOverclocked OnConstructOverclocked;

	UPROPERTY(EditAnywhere, Category = "Kaelen|Data")
	TObjectPtr<UDataTable> ConstructDataTable;

	// 0 = unlimited simultaneous constructs.
	UPROPERTY(EditAnywhere, Category = "Kaelen|Config")
	int32 MaxSimultaneousConstructs = 1;

protected:
	virtual void BeginPlay() override;

private:
	const FConstructDefinition* FindDefinition(FName ConstructId) const;
	FActiveConstruct* FindActive(FName ConstructId);
	TScriptInterface<class IKaelenResourcePool> ResolveResourcePool() const;
	void DeconstructAll(bool bForced);

	UPROPERTY()
	TObjectPtr<UThreadInventoryComponent> ThreadInventory;

	UPROPERTY()
	TArray<FActiveConstruct> ActiveConstructs;
};
