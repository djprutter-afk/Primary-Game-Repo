using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using UnityEngine.Tilemaps;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Mono.Cecil;
using UnityEngine.EventSystems;
using System.Numerics;
//this is all made with tutorial: https://www.youtube.com/watch?v=T_sBYgP7_2k&t=613s 

public interface iActionStrat
{
    bool canPerform { get; }
    bool complete { get; }
    void Start();
   
    void Update(float deltaTime){}
    
    void Stop(){}
    
}



public class waitTickStrat : iActionStrat
{

    int ticksRemaining;
    int fortniteTicks;

    public bool canPerform => !complete;
    public bool complete => ticksRemaining <= 0;
    public waitTickStrat(int ticks)
    {
        fortniteTicks = ticks;
        
    }
    void tick()
    {
        ticksRemaining--;

    }
    public void Start()
    {
        ticksRemaining = fortniteTicks;
       gameManagerScript.GameTick += this.tick; 
    }




}



public class makeSpaceStrat : iActionStrat
{
    bool foundSpotToMove = false;
    public bool canPerform => !complete;
    public bool complete => foundSpotToMove;
    colonyScript localColony;

    public makeSpaceStrat(colonyScript colony)
    {
        localColony = colony;
    }
        
    public void Start()
    {
        GameObject[] buildables =  localColony.ownedBuildables.ToArray();
        int[] order =  playerMouseInteractions.randomAssortment(buildables.Length);
        for(int i = 0; i <buildables.Length;i++)
        {
            buildableScript thisBuildableScript = buildables[order[i]].GetComponent<buildableScript>();
            if(thisBuildableScript.isBuilding == true)
            {
                continue;
            }
            GameObject theTileWhichTheBuildableIsOn = thisBuildableScript.tileOn;
            Collider[] surroundingTiles = Physics.OverlapSphere(theTileWhichTheBuildableIsOn.transform.position, 0.05f);
            foreach(Collider currentTile in surroundingTiles)
            {
                tileInfo tileOnInfo = currentTile.GetComponent<tileInfo>();
                if(tileOnInfo == null)
                {
                    continue;
                }
                if(tileOnInfo.occupid == false)
                {
                    bool succeded = thisBuildableScript.buildableAction(buildableScript.buildableActions.Move,currentTile.gameObject);
                    
                    
                   
                }
            }
            
            
        }
        foundSpotToMove = true; // just incase there were no spots found, redo the script if this becomes a problem
        
    }
}


public class chooseAndBuildBuildableStrat : iActionStrat
{
    bool builtTheThing;
    public bool canPerform => !complete;
    public bool complete => builtTheThing;
    baseColonyAI colonyAI;
    int amountToBuild;
buildableScript.AIBuildableInfo.buildablePurposes[] specificPurposes;
    public chooseAndBuildBuildableStrat(baseColonyAI sgfa, buildableScript.AIBuildableInfo.buildablePurposes[] SpecificPurposes  = null, int AmountToBuild = 1)
    {
      colonyAI =sgfa;
    specificPurposes = SpecificPurposes;
    amountToBuild = AmountToBuild;
                 
    }
    public chooseAndBuildBuildableStrat(baseColonyAI sgfa, buildableScript.AIBuildableInfo.buildablePurposes SpecificPurposes, int AmountToBuild = 1)
    {
      colonyAI =sgfa;
    specificPurposes =new buildableScript.AIBuildableInfo.buildablePurposes[]{ SpecificPurposes};
    amountToBuild = AmountToBuild;
                 
    }
    public void Start()
    {
        

       
        
        
       buildableGameObject objectBuild = bestBuildableToBuild();
       buildChosen(objectBuild,amountToBuild);
        builtTheThing = true;
     
        
    
       
    }

    buildableGameObject bestBuildableToBuild()
    {
         buildableScript.AIBuildableInfo.buildablePurposes[] purposeWanted = null;
        if(specificPurposes == null)
        { 
            purposeWanted = (buildableScript.AIBuildableInfo.buildablePurposes[])Enum.GetValues(typeof(buildableScript.AIBuildableInfo.buildablePurposes));// chose the most desired type at the time. ideally this should never happen
            
        }
        else
        {
           purposeWanted = specificPurposes;
        }
       

        colonyScript colonyScript = colonyAI.thisColonyScript;
       List<KeyValuePair<buildableGameObject,float>> potentialBuildables =new List<KeyValuePair<buildableGameObject,float>>();
        TriValueStruct colonyIncome = colonyScript.totalIncome();

        foreach(buildableGameObject buildableGameObject in gameManagerScript.allBuildables)
        {

            buildableScript buildableScript = buildableGameObject.buildableObject.GetComponent<buildableScript>();
            float totalValue = 0;
            bool containsDesiredPurpose = false;
            for(int i = 0; i < buildableScript.purposes.Length;i++)
            {
                totalValue += evaluatePurposeToColony(buildableScript.purposes[i],colonyAI);
                if(purposeWanted.Contains(buildableScript.purposes[i].purpose)==true)
                {
                    containsDesiredPurpose = true;
                }
                
            }
            totalValue-= totalBurdenOfBuildable(buildableGameObject,colonyIncome);

            totalValue = Mathf.Max(0.01f,totalValue);
            KeyValuePair<buildableGameObject,float> kvp = new  KeyValuePair<buildableGameObject,float>(buildableGameObject,totalValue);
            if(containsDesiredPurpose == true || purposeWanted.Length <=0)
            {
                potentialBuildables.Add(kvp);
            }
            
        } 
        float total = 0f;
        foreach (var kvp in potentialBuildables)
        {
              total += kvp.Value;

        }
          
        float roll = UnityEngine.Random.Range(0f, total);

        float cumulative = 0f;
        foreach (var kvp in potentialBuildables)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
            {
                 return kvp.Key;
            }
               
        }

    
      
       
        // nuclear level bad, should never ever happen
    Debug.LogWarning("no buildable found");
        builtTheThing= true; 
        return null;

        
        
        
     
    }
    float evaluatePurposeToColony(buildableScript.AIBuildableInfo.biInfoStuct purpose, baseColonyAI colonyAI)
    {
        return colonyAI.valueOfBuildables[purpose.purpose] * purpose.strength;
    }  
    float totalBurdenOfBuildable(buildableGameObject buildableGameObject,TriValueStruct colonyIncome)
    {
        float maxPotentialResourceExtraction = colonyAI.MaxResourceExtraction();
       
        buildableScript buildable = buildableGameObject.buildableObject.GetComponent<buildableScript>();
        TriValueStruct buildableUpkeep = buildable.upkeepCosts;
        float totalBurden = costEvaluation(buildableUpkeep,colonyIncome,maxPotentialResourceExtraction) + costEvaluation(buildableGameObject.buildCost,colonyIncome,float.MaxValue)/2.5f; // buildableupkeep should be weighted more cause ai will have to live with it longer
        return totalBurden;
    }
    
    float costEvaluation(TriValueStruct changeAmount,TriValueStruct initalValue,float maxExtraction)
    {
       float doubleNegativeBurden = -2;
       
       float dNBMoney = 1;
       if(changeAmount.moneyValue<0 && initalValue.moneyValue<0){dNBMoney = doubleNegativeBurden;}

       float dNBResource = 1;
       if(changeAmount.resourceValue<0 && initalValue.resourceValue<0){dNBResource = doubleNegativeBurden;}

       float dNBPopulation =1;
       if(changeAmount.populationValue<0 && initalValue.populationValue<0){dNBPopulation = doubleNegativeBurden;}
        

        if(changeAmount.resourceValue < 0)
        {
            changeAmount.resourceValue = -Mathf.Min(Mathf.Abs(changeAmount.resourceValue),maxExtraction);
        }
       
       TriValueStruct incomeChange = initalValue.subtract(changeAmount);
       TriValueStruct safeInitial = initalValue.addition(TriValueStruct.one.multiply(0.01f));

       TriValueStruct changePercent = incomeChange.divide(safeInitial);
       return (1- changePercent.moneyValue) *dNBMoney + (1-changePercent.resourceValue)*dNBResource + (1-changePercent.populationValue)*dNBPopulation;
      
    }
    
    void buildChosen(buildableGameObject buildableGameObject, int amountToBuildChosen)
    {
        List<tileInfo> tileCandidates = new List<tileInfo>();
        foreach(tileInfo tile in colonyAI.thisColonyScript.allTilesOwned.Select(x=>x.GetComponent<tileInfo>()))
        {
            if(tile.occupid == false)
            {
                tileCandidates.Add(tile);
            }
        }
        if(tileCandidates.Count() <= 0)// should never happen because having space is a precondition for this strat
        {
            return;
        }
        TriValueStruct wealthOwned = colonyAI.thisColonyScript.resourcesOwned;

        int amountCanAfford = 1;
        for(int i = 0; i <amountToBuild;i++)
        {
            Debug.Log("checking if null: weathOwned is null? " + wealthOwned.ToString());
           if(buildableGameObject.IsUnityNull())
            {
                Debug.LogError("buildableGameObject is null!");
                return;
            }
            bool canAfford = TriValueStruct.comapareCosts(wealthOwned,buildableGameObject.buildCost.multiply(amountToBuild - i));
            if(canAfford == true)
            {
                amountCanAfford = amountToBuild - i;
            }
        
        }
        if(tileCandidates.Count() < amountCanAfford)
        {
            amountCanAfford = tileCandidates.Count() ;
        }
        for(int i=0; i < amountCanAfford; i++)
        {
            GameObject gameObjectBuild = tileCandidates[i].buildNewBuildable(buildableGameObject,colonyAI.thisColonyScript);
            if(i == amountCanAfford-1)
            {
                buildableScript buildableScript = gameObjectBuild.GetComponent<buildableScript>();
                buildableScript.doneCreatingSelf += finished;
                

            }

        }

   
        
    }


    void finished()
    {
           builtTheThing = true;
           
    }
   

    
       
}
