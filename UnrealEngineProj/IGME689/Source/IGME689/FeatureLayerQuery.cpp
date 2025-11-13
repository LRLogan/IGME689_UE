// Fill out your copyright notice in the Description page of Project Settings.


#include "FeatureLayerQuery.h"

#include <rapidjson/reader.h>

#include "InstanceDataSceneProxy.h"

// Sets default values
AFeatureLayerQuery::AFeatureLayerQuery()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void AFeatureLayerQuery::BeginPlay()
{
	Super::BeginPlay();
	ProcessRequest();
}

// Called every frame
void AFeatureLayerQuery::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

void AFeatureLayerQuery::OnResponceReceived(FHttpRequestPtr request, FHttpResponsePtr response, bool bWasSuccessful)
{
	if (!bWasSuccessful)
	{
		return;
	}
	
	TSharedPtr<FJsonObject> responceObj;
	const auto responceBody = response->GetContentAsString();
	auto reader = TJsonReaderFactory<>::Create(responceBody);
	
	UE_LOG(LogTemp, Warning, TEXT("%s"), *responceBody);
	
	if (FJsonSerializer::Deserialize(reader, responceObj))
	{
		auto featureObjects = responceObj->GetArrayField(TEXT("features"));
		
		for (auto feature : featureObjects)
		{
			FProperties curFeature;
			auto coords = feature->AsObject()->GetObjectField(TEXT("geometry"))->
			GetArrayField(TEXT("coordinates"));
			
			auto properties = feature->AsObject()->GetObjectField(TEXT("properties"))->
			Values; 
			
			// Getting properties
			for (auto property : properties)
			{
				curFeature.properties.Add(property.Value->AsString());
			}
			
			// Getting geometries 
			for (int i = 0; i < coords.Num(); i++)
			{
				auto thisGeo = coords[i]->AsArray();
				FGeometries geometry;
				geometry.geometry.Add(thisGeo[0]->AsNumber());	// Lat
				geometry.geometry.Add(thisGeo[1]->AsNumber()); // Long
				curFeature.geometries.Add(geometry);
			}
			features.Add(curFeature);
		}
	}
}

void AFeatureLayerQuery::ProcessRequest()
{
	FHttpRequestRef Request = FHttpModule::Get().CreateRequest();
	Request->OnProcessRequestComplete().BindUObject(this, &AFeatureLayerQuery::OnResponceReceived);
	Request->SetURL(webLink);
	Request->SetVerb("Get");
	Request->ProcessRequest();
}

