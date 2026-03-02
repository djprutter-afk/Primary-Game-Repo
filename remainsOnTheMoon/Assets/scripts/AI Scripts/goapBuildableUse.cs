using UnityEngine;

using System.Collections.Generic;

using System.Linq;
using System;
using Unity.Collections;

public enum thingsToTargetEnum
{
    tiles,
    buildables,
    both
}
public abstract class buildableUseStrat : iActionStrat
{
    bool finishedAction = false;
    public bool canPerform => !complete;
    public bool complete => finishedAction;
    public baseColonyAI colonyAI;
buildableScript.AIBuildableInfo.buildablePurposes buildablePurposeNeeded;
buildableScript.buildableActions actionToUse;


    thingsToTargetEnum whatToTarget;
    public  buildableUseStrat(baseColonyAI ColonyAI, buildableScript.AIBuildableInfo.buildablePurposes BuildablePurposeNeeded,buildableScript.buildableActions actionToUse,thingsToTargetEnum WhatToTarget)
    {
        colonyAI = ColonyAI;
        buildablePurposeNeeded = BuildablePurposeNeeded;
        this.actionToUse = actionToUse;
        whatToTarget = WhatToTarget;
        
      
    }
    public virtual UnityEngine.Vector3[] postionDecider()
    {
        UnityEngine.Vector3[] position = new UnityEngine.Vector3[1];

        position[0] = UnityEngine.Vector3.zero;
        return position;
    }
    public void Start()
    {
        Dictionary<string,float> aIPressures = new Dictionary<string, float>();
        foreach(AgentGoal goal in colonyAI.goals)
        {
            aIPressures.Add(goal.Name,goal.priority);
        }
        float dedicationAmount = 0.1f; // the percentage of buildables of the purpose defined above of that the ai dedicates to this action. 0 means none, 1 means all
        void  deciationStrengthCalc(string agentGoal)
        {
            float currentValue = aIPressures[agentGoal];
            if(currentValue >= dedicationAmount)
            {
                dedicationAmount = currentValue;
            }
        }

        switch (buildablePurposeNeeded)
        {
            case buildableScript.AIBuildableInfo.buildablePurposes.expansion:
                 deciationStrengthCalc("expansionPressure");
                
            break;

            case buildableScript.AIBuildableInfo.buildablePurposes.offensive:
                deciationStrengthCalc("militaryPressure");
            break;

            case buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive:
                deciationStrengthCalc("militaryPressure");
            break;

            case buildableScript.AIBuildableInfo.buildablePurposes.defensive:
                deciationStrengthCalc("militaryPressure");
            break;

            case buildableScript.AIBuildableInfo.buildablePurposes.antiMissile:
                deciationStrengthCalc("militaryPressure");
            break;
             case buildableScript.AIBuildableInfo.buildablePurposes.economy:
                 deciationStrengthCalc("economyPressure");
            break;

            default:
            Debug.LogError("no goal  is related with purpose, CHECK CODE");
            break;
            
            
        }
        
        
        buildableScript[] buildablesOfPurpose = colonyAI.thisColonyScript.ownedBuildables.Select(b => b.GetComponent<buildableScript>()).Where(buildable => buildable.purposes.Any(purpose => purpose.purpose == buildablePurposeNeeded)).ToArray();
        float totalValueOfPurpose = 0;
        Dictionary<buildableScript,float> valueOfBuildable = new Dictionary<buildableScript, float>();
        foreach(var builable in buildablesOfPurpose)
        {
            foreach(var purpsoe in builable.purposes)
            {
                if(purpsoe.purpose == buildablePurposeNeeded)
                {
                    valueOfBuildable.Add(builable,purpsoe.strength);
                    totalValueOfPurpose += purpsoe.strength;
                    break;
                    
                }
            }
        }
        dedicationAmount = Mathf.Clamp(dedicationAmount,0,1);
        float valueNeed = totalValueOfPurpose * dedicationAmount;
        float currentValueUsed =0f;
       
        UnityEngine.Vector3[] positionsToUse = postionDecider();
        if(positionsToUse.Length == 0)
        {
             finishedAction = true;
    return;
        }

        int amountOfBuildables = valueOfBuildable.Count();

        int targetsPerPosition = Mathf.CeilToInt((float)amountOfBuildables / positionsToUse.Length) + 1;; // plus 1 just to be safe, it doesnt effect anything if all goes well
        
        Dictionary<UnityEngine.Vector3,GameObject[]> tileClosestToPosition = new Dictionary<UnityEngine.Vector3, GameObject[]>();  
        foreach(UnityEngine.Vector3 position in positionsToUse)
        {
            for(int i = 0; i <10; i++)
            {
                Collider[] hitColliders = Physics.OverlapSphere(position, i * 0.2f);// will attempt 10 times to find enough tiles, it will go up until searching are is daiamter of the moon



                  GameObject[]  objectToTarget = null;
                GameObject[] targetTypes(Type[] types)
                {
                    List<GameObject> GameObjectsToFind = new List<GameObject>();
                    foreach(Type type in types)
                    {
                         GameObjectsToFind.AddRange( hitColliders.Select(x=>x.gameObject).Where(x=>x.TryGetComponent(type, out Component Tile) ==true).ToArray());
                    }
                    return GameObjectsToFind.ToArray();

                
                
                }
                 GameObject[] targetType(Type type)
                {
                    List<GameObject> GameObjectsToFind = new List<GameObject>();
                 
                    GameObjectsToFind.AddRange( hitColliders.Select(x=>x.gameObject).Where(x=>x.TryGetComponent(type, out Component Tile) ==true).ToArray());
                    
                    return GameObjectsToFind.ToArray();

                
                
                }
              

                switch (whatToTarget)
                {
                    case thingsToTargetEnum.tiles:
                    objectToTarget = targetType(typeof(tileInfo));
                    break;
                    case thingsToTargetEnum.buildables:
                    objectToTarget = targetType(typeof(buildableScript));
                    break;
                    case thingsToTargetEnum.both:
                    objectToTarget = targetTypes(new Type[2]{typeof(tileInfo),typeof(buildableScript)});


                    break;
                }
                
                 
               

                if(objectToTarget.Length >targetsPerPosition)
                {

                    tileClosestToPosition.Add(position,objectToTarget.Take(targetsPerPosition).ToArray());
                    break;
                    
                }
            }
            
            
           
        }
        bool atLeastOneUsed = false;// although an action my not be worth sending literally anything out, it should still happen cause the ai expects something to happeb
       for(int i=0; i <targetsPerPosition;i++)
        {
            
             foreach(var position in tileClosestToPosition)
            {
            
                KeyValuePair<buildableScript,float>  bestBuildable = buildableDecider(position.Key,valueOfBuildable);
                if(bestBuildable.Value == 0)
                {
                    break;
                }
                if(currentValueUsed + bestBuildable.Value < valueNeed || atLeastOneUsed == false)
                {

                   
                    bestBuildable.Key.buildableAction(actionToUse,position.Value[i]);
                    currentValueUsed += bestBuildable.Value;
                    bestBuildable.Key.finishedAction += hasFinishedAction;

                    atLeastOneUsed = true;
                    expectedAmountToFinish++;


                }
                valueOfBuildable.Remove(bestBuildable.Key);



            }

        }
        
       
              
            
        
       



      
       
    }
    KeyValuePair<buildableScript,float> buildableDecider(UnityEngine.Vector3 positionToCheck,Dictionary<buildableScript,float> buildableScriptKVP)
    {
        
        KeyValuePair<buildableScript,float> currentBest = new KeyValuePair<buildableScript, float>();
        foreach(var buildable in buildableScriptKVP)
        {

            float distance = UnityEngine.Vector3.Distance(buildable.Key.transform.position,positionToCheck);
            
            
            
            float currentValue = Mathf.Clamp(buildable.Value * (2- distance),0,2);// the moon's diameter is 2 units across
            

            if(currentValue >= currentBest.Value &&distance <= buildable.Key.possibleRangeDiameter/2)
            {
                currentBest = buildable;
            }
            
            
        }

      
        
        return currentBest;
    }
    int expectedAmountToFinish;
    int totalFinished;

    void hasFinishedAction()
    {
        totalFinished++;
        if(totalFinished >= expectedAmountToFinish)
        {
             finishedAction = true;
        }
       

    }
    
}

/*
template
public class TEMPLATESTRAT : buildableUseStrat
{
   public TEMPLATESTRAT(baseColonyAI colonyAI, buildableScript.AIBuildableInfo.buildablePurposes purpose, buildableScript.buildableActions actionToUse) // add whatever params here
    : base(colonyAI, purpose, actionToUse,true)
    {
        push args into variables
    
    }
    public override Vector3[] postionDecider() // if this isnt there then anything is game
    {
        do work
        return something
    }


}


*/
public class settlerUseStrat : buildableUseStrat
{

    int positionsToFind;
        public settlerUseStrat(baseColonyAI colonyAI, buildableScript.AIBuildableInfo.buildablePurposes purpose,buildableScript.buildableActions actionToUse,int PositionsToFind,thingsToTargetEnum thingsToTarget)
        : base(colonyAI, purpose, actionToUse,thingsToTarget)
    {
        positionsToFind = PositionsToFind;
       
    }
    public override Vector3[] postionDecider()
    {
       
         GameObject[] sortedGameObjects = new GameObject[positionsToFind];// end result

        colonyScript colonyScript = colonyAI.gameObject.GetComponent<colonyScript>();
        GameObject[] outlineTiles = colonyMethoods.findOUterEdgeTiles(colonyAI.gameObject);
        int numberOfOutlineTiles = outlineTiles.Length;

        if(positionsToFind > outlineTiles.Length)
        {
            positionsToFind =  outlineTiles.Length;
        }


        if (numberOfOutlineTiles <= 0)
        {
            Debug.LogError("not enough tiless to evalute");
            return null;
        }
        Dictionary<GameObject, float> dictonaryOfTiles = new Dictionary<GameObject, float>();



        for (int indexOfOutlineTiles = 0; indexOfOutlineTiles < numberOfOutlineTiles; indexOfOutlineTiles++)
        {
            float tileValue = 0;
            GameObject currentOutLineTile = outlineTiles[indexOfOutlineTiles];

            tileInfo currentTileInfo = currentOutLineTile.GetComponent<tileInfo>();

            tileValue += currentTileInfo.resource;

            Collider[] tilesSurroundingCurrent = Physics.OverlapSphere(currentOutLineTile.transform.position, 0.05f);


            for (int k = 0; k < tilesSurroundingCurrent.Length; k++)
            {


                if (tilesSurroundingCurrent[k].transform.parent.gameObject == colonyAI.transform.gameObject)
                {

                    tileValue += (1 - colonyAI.aggression) * 2;

                }
            }
          
            if (dictonaryOfTiles.ContainsKey(currentOutLineTile) == false && colonyScript.allTilesOwned.Contains(currentOutLineTile) == false)
            {
                dictonaryOfTiles.Add(currentOutLineTile, tileValue);

            }
        }

            List<KeyValuePair<GameObject, float>> kvpOfTiles = dictonaryOfTiles.ToList();

            var sortedKvpOfTiles = kvpOfTiles.OrderByDescending(pair => pair.Value).ToList();

           

            for(int i = 0; i < positionsToFind ;i++)
            {
                sortedGameObjects[i] = sortedKvpOfTiles[i].Key;
            }

          

        
         return sortedGameObjects.Select(x=>x.transform.position).ToArray();


    }
 




    
}

public class bombEnemiesStrat : buildableUseStrat
{

    float desireThreshold; 
   public bombEnemiesStrat(baseColonyAI colonyAI, buildableScript.AIBuildableInfo.buildablePurposes purpose, buildableScript.buildableActions actionToUse,thingsToTargetEnum whatToTarget,float DesireThreshold) // add whatever params here
    : base(colonyAI, purpose, actionToUse,whatToTarget)
    {
        desireThreshold = DesireThreshold;
    
    }
    public override Vector3[] postionDecider()
    {

        List<colonyScript> coloniesToAttack = new List<colonyScript>();
        KeyValuePair<colonyScript,float> bestColony = new KeyValuePair<colonyScript, float>();
        foreach(baseColonyAI.otherColonyInfo colonyInfo in colonyAI.otherColonyInfos)
        {
            float desireToAttack = (1-colonyInfo.friendliness) * colonyInfo.threatLevel;
            if(bestColony.Value < desireToAttack)
            {
                bestColony = new KeyValuePair<colonyScript, float>(colonyInfo.colony,desireToAttack);
            }

            if(desireToAttack>= desireThreshold)
            {
                coloniesToAttack.Add(colonyInfo.colony);
            }


        }
        if(coloniesToAttack.Count == 0)
        {
            coloniesToAttack.Add(bestColony.Key); // again, even if it goes against the grain the ai willed it to happen, and thus something must happen
        }
        List<Vector3> centerOfEnemies = new List<Vector3>();
        foreach(var colony in coloniesToAttack)
        {

             Vector3 averagePosition = Vector3.zero;
             foreach(GameObject tile in colony.allTilesOwned)
             {
                 averagePosition += tile.transform.position;
             }
            centerOfEnemies.Add(averagePosition /= colony.allTilesOwned.Count) ;

           
        }

        return centerOfEnemies.ToArray();


       
    }


}