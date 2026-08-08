#include "MagicTypes.h"

FName MintElemental::GetWorldInteractionTagForColor(EMagicColor Color)
{
	switch (Color)
	{
	case EMagicColor::Red:   return TEXT("IgniteBrazier");
	case EMagicColor::Blue:  return TEXT("FreezeWater");
	case EMagicColor::Green: return TEXT("DriveTurbine");
	case EMagicColor::Black: return TEXT("ShatterBarrier");
	default:                 return NAME_None;
	}
}
