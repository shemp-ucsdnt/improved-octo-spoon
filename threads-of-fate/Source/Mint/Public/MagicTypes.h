#pragma once

#include "CoreMinimal.h"
#include "MagicTypes.generated.h"

// Colors are a closed set: each maps 1:1 to a fixed world interaction
// (Red ignites braziers, Blue freezes water, Green drives turbines, Black
// shatters barriers — see MintElemental::GetWorldInteractionTagForColor).
// Effects (Burner, Restore, ...) are an open, data-driven set, so they are
// identified by FName in FSpellDefinition rather than enumerated here.
UENUM(BlueprintType)
enum class EMagicColor : uint8
{
	Red,
	Blue,
	Green,
	Black
};

namespace MintElemental
{
	FName GetWorldInteractionTagForColor(EMagicColor Color);
}
