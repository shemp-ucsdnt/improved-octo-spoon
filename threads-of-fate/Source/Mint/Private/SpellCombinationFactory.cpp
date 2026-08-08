#include "SpellCombinationFactory.h"

void USpellCombinationFactory::Initialize(UDataTable* InSpellTable)
{
	SpellDataTable = InSpellTable;
	RebuildLookup();
}

void USpellCombinationFactory::RebuildLookup()
{
	Lookup.Reset();

	if (!SpellDataTable)
	{
		return;
	}

	SpellDataTable->ForeachRow<FSpellDefinition>(TEXT("RebuildLookup"),
		[this](const FName& RowName, const FSpellDefinition& Row)
		{
			Lookup.Add(FSpellKey{ Row.Color, Row.Effect }, Row);
		});
}

bool USpellCombinationFactory::CombineSpell(EMagicColor Color, FName Effect, FSpellDefinition& OutSpell) const
{
	if (const FSpellDefinition* Found = Lookup.Find(FSpellKey{ Color, Effect }))
	{
		OutSpell = *Found;
		return true;
	}
	return false;
}

bool USpellCombinationFactory::IsValidCombination(EMagicColor Color, FName Effect) const
{
	return Lookup.Contains(FSpellKey{ Color, Effect });
}
