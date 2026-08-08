#include "ConstructDeploymentManager.h"
#include "ThreadInventoryComponent.h"
#include "KaelenResourcePoolInterface.h"
#include "GameFramework/Actor.h"

UConstructDeploymentManager::UConstructDeploymentManager()
{
	PrimaryComponentTick.bCanEverTick = true;
}

void UConstructDeploymentManager::BeginPlay()
{
	Super::BeginPlay();

	if (AActor* Owner = GetOwner())
	{
		ThreadInventory = Owner->FindComponentByClass<UThreadInventoryComponent>();
	}
}

const FConstructDefinition* UConstructDeploymentManager::FindDefinition(FName ConstructId) const
{
	if (!ConstructDataTable)
	{
		return nullptr;
	}
	return ConstructDataTable->FindRow<FConstructDefinition>(ConstructId, TEXT("FindDefinition"));
}

FActiveConstruct* UConstructDeploymentManager::FindActive(FName ConstructId)
{
	return ActiveConstructs.FindByPredicate([ConstructId](const FActiveConstruct& Active)
	{
		return Active.Definition.ConstructId == ConstructId;
	});
}

TScriptInterface<IKaelenResourcePool> UConstructDeploymentManager::ResolveResourcePool() const
{
	AActor* Owner = GetOwner();
	if (Owner && Owner->Implements<UKaelenResourcePool>())
	{
		return TScriptInterface<IKaelenResourcePool>(Owner);
	}
	return nullptr;
}

bool UConstructDeploymentManager::IsConstructActive(FName ConstructId) const
{
	return ActiveConstructs.ContainsByPredicate([ConstructId](const FActiveConstruct& Active)
	{
		return Active.Definition.ConstructId == ConstructId;
	});
}

bool UConstructDeploymentManager::DeployConstruct(FName ConstructId)
{
	if (MaxSimultaneousConstructs > 0 && ActiveConstructs.Num() >= MaxSimultaneousConstructs)
	{
		return false;
	}

	const FConstructDefinition* Definition = FindDefinition(ConstructId);
	if (!Definition || !ThreadInventory)
	{
		return false;
	}

	if (!ThreadInventory->ConsumeThreads(Definition->RequiredThreads))
	{
		return false;
	}

	FActiveConstruct Active;
	Active.Definition = *Definition;
	Active.State = EConstructState::Active;
	ActiveConstructs.Add(MoveTemp(Active));

	OnConstructDeployed.Broadcast(ConstructId);
	return true;
}

bool UConstructDeploymentManager::Overclock(FName ConstructId)
{
	FActiveConstruct* Active = FindActive(ConstructId);
	if (!Active || Active->State != EConstructState::Active)
	{
		return false;
	}

	TScriptInterface<IKaelenResourcePool> ResourcePool = ResolveResourcePool();
	if (!ResourcePool)
	{
		return false;
	}

	const float MPCost = Active->Definition.OverclockMPCost;
	const float KineticCost = Active->Definition.OverclockKineticEnergyCost;

	if (IKaelenResourcePool::Execute_GetCurrentMP(ResourcePool.GetObject()) < MPCost
		|| IKaelenResourcePool::Execute_GetKineticEnergy(ResourcePool.GetObject()) < KineticCost)
	{
		return false;
	}

	if (!IKaelenResourcePool::Execute_ConsumeMP(ResourcePool.GetObject(), MPCost)
		|| !IKaelenResourcePool::Execute_ConsumeKineticEnergy(ResourcePool.GetObject(), KineticCost))
	{
		return false;
	}

	OnConstructOverclocked.Broadcast(ConstructId, Active->Definition.OverclockAbilityId);
	return true;
}

void UConstructDeploymentManager::Deconstruct(FName ConstructId, bool bForced)
{
	const int32 Index = ActiveConstructs.IndexOfByPredicate([ConstructId](const FActiveConstruct& Active)
	{
		return Active.Definition.ConstructId == ConstructId;
	});

	if (Index == INDEX_NONE)
	{
		return;
	}

	FActiveConstruct& Active = ActiveConstructs[Index];
	Active.State = EConstructState::Deconstructing;

	if (Active.Definition.bRefundThreadsOnDeconstruct && ThreadInventory)
	{
		ThreadInventory->RefundThreads(Active.Definition.RequiredThreads);
	}

	if (AActor* Spawned = Active.SpawnedActor.Get())
	{
		Spawned->Destroy();
	}

	ActiveConstructs.RemoveAt(Index);
	OnConstructDeconstructed.Broadcast(ConstructId, bForced);
}

void UConstructDeploymentManager::DeconstructAll(bool bForced)
{
	TArray<FName> Ids;
	Ids.Reserve(ActiveConstructs.Num());
	for (const FActiveConstruct& Active : ActiveConstructs)
	{
		Ids.Add(Active.Definition.ConstructId);
	}
	for (const FName& Id : Ids)
	{
		Deconstruct(Id, bForced);
	}
}

void UConstructDeploymentManager::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (ActiveConstructs.Num() == 0)
	{
		return;
	}

	TScriptInterface<IKaelenResourcePool> ResourcePool = ResolveResourcePool();
	if (!ResourcePool)
	{
		return;
	}

	float TotalDrain = 0.f;
	for (const FActiveConstruct& Active : ActiveConstructs)
	{
		TotalDrain += Active.Definition.MaintenanceMPPerSecond * DeltaTime;
	}

	if (TotalDrain > 0.f)
	{
		IKaelenResourcePool::Execute_ConsumeMP(ResourcePool.GetObject(), TotalDrain);
	}

	if (IKaelenResourcePool::Execute_GetCurrentMP(ResourcePool.GetObject()) <= 0.f)
	{
		DeconstructAll(/*bForced=*/true);
	}
}
