// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Http.h"
#include "FeatureLayerQuery.generated.h"

// Holds the indevidual geos
USTRUCT(BlueprintType)
struct FGeometries
{
	GENERATED_BODY()
	
public:
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere);
	TArray<float> geometry;
};

// Holds the overarching property
USTRUCT(BlueprintType)
struct FProperties
{
	GENERATED_BODY()
	
public:
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere, Category="Properties");
	int32 alt;
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere, Category="Properties");
	int32 length;
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere, Category="Properties");
	FString location;
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere, Category="Properties");
	FString name;
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere, Category="Geometry");
	TArray<FGeometries> geometries;
};

UCLASS()
class IGME689_API AFeatureLayerQuery : public AActor
{
	GENERATED_BODY()
	
	
	
public:	
	// Sets default values for this actor's properties
	AFeatureLayerQuery();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:	
	// Called every frame
	virtual void Tick(float DeltaTime) override;
	virtual void OnResponceReceived(FHttpRequestPtr request, FHttpResponsePtr response, bool bWasSuccessful);
	virtual void ProcessRequest();
	
	UPROPERTY(BlueprintReadOnly, VisibleAnywhere)
	TArray<FProperties> features;
	
	
private:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (AllowPrivateAccess = "true"))
	// Cur link for F1 race tracks:
	// https://services2.arcgis.com/yL7v93RXrxlqkeDx/arcgis/rest/services/F1_World_Championship_Circuits/FeatureServer/0/query?f=geojson&where=1=1&outfields=*
	FString webLink = "";
};
