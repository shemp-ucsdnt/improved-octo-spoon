#pragma once

#include "CoreMinimal.h"
#include "Engine/DataTable.h"
#include "MagicTypes.h"
#include "SpellCombinationFactory.generated.h"

USTRUCT(BlueprintType)
struct FSpellKey
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Mint|Spells")
	EMagicColor Color = EMagicColor::Red;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Mint|Spells")
	FName Effect;

	bool operator==(const FSpellKey& Other) const
	{
		return Color == Other.Color && Effect == Other.Effect;
	}
};

FORCEINLINE uint32 GetTypeHash(const FSpellKey& Key)
{
	return HashCombine(GetTypeHash(Key.Color), GetTypeHash(Key.Effect));
}

// Data-table row: one row per valid Color+Effect combination
// (e.g. Red+Burner = Flamethrower, Blue+Restore = Full HP Heal).
USTRUCT(BlueprintType)
struct FSpellDefinition : public FTableRowBase
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Recipe")
	EMagicColor Color = EMagicColor::Red;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Recipe")
	FName Effect;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Spell")
	FText SpellName;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Spell")
	float MPCost = 0.f;

	// Resolved by the combat system to a GameplayAbility/effect class.
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Spell")
	FName AbilityId;
};

// Strategy/Factory for Mint's Color x Effect magic matrix: each data-table
// row is a strategy, keyed by (Color, Effect) for O(1) lookup at cast time.
UCLASS(BlueprintType)
class USpellCombinationFactory : public UObject
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category = "Mint|Spells")
	void Initialize(UDataTable* InSpellTable);

	UFUNCTION(BlueprintCallable, Category = "Mint|Spells")
	bool CombineSpell(EMagicColor Color, FName Effect, FSpellDefinition& OutSpell) const;

	UFUNCTION(BlueprintPure, Category = "Mint|Spells")
	bool IsValidCombination(EMagicColor Color, FName Effect) const;

private:
	void RebuildLookup();

	UPROPERTY()
	TObjectPtr<UDataTable> SpellDataTable;

	TMap<FSpellKey, FSpellDefinition> Lookup;
};
