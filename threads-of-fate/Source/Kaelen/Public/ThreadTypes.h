#pragma once

#include "CoreMinimal.h"
#include "ThreadTypes.generated.h"

UENUM(BlueprintType)
enum class EThreadElement : uint8
{
	Beast,
	Iron,
	Fire,
	Wind,
	Lightning,
	Water,
	Phantom,
	Crystal
};

USTRUCT(BlueprintType)
struct FThreadCost
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Threads")
	EThreadElement Element = EThreadElement::Beast;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Threads")
	int32 Count = 1;
};
