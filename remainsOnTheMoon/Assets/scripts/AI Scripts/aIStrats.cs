using UnityEngine;


using System.Collections.Generic;

using System.Linq;

public class aIStrats
{
    


    public interface iActionStrat
{
    bool canPerform { get; }
    bool complete { get; }
    void Start();
   
    void Update(float deltaTime){}
    
    void Stop(){}
    
}
/// <summary>
/// should be removed
/// </summary>




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


public class buildStrat : iActionStrat
{
    buildableScript trackingBuildable; 
    bool finished = false;
    BuildingStruct purchciceCost;
    colonyScript callingColony;
    GameObject deathObject;
    buildableScript thrbuildableScript;

    public bool canPerform => callingColony != null && BuildingStruct.comapareCosts(callingColony.resourcesOwned, purchciceCost);
    public bool complete => finished;
    GameObject targetPos;
    int amountTobuild;
    baseColonyAI myAI;
    public buildStrat(GameObject sdfsdf,  baseColonyAI fdg)
    {
        amountTobuild = 1;// make better later
        targetPos = sdfsdf;
        callingColony = fdg.thisColonyScript;
        
        
 
        myAI = fdg;





    }
    public void Start()
    { thrbuildableScript =myAI.desiredBuildable.buildableObject.GetComponent<buildableScript>();
        deathObject = myAI.desiredBuildable.buildableObject;
        purchciceCost = myAI.desiredBuildable.buildCost;
        GameObject[] ownedTiles = callingColony.allTilesOwned.ToArray();
        Dictionary<GameObject, float> tileDic = new Dictionary<GameObject, float>();
        foreach (GameObject currentTile in ownedTiles)
        {
            tileInfo currentTileInfo = currentTile.GetComponent<tileInfo>();

            if (currentTileInfo.occupid == true)
            {
                continue;
            }
            float distance = Vector3.Distance(targetPos.transform.position, currentTile.transform.position);
            tileDic.Add(currentTile, distance);
        }

        int amountBuilded = 0;

        foreach (KeyValuePair<GameObject, float> tileKVP in tileDic.OrderBy(x => x.Value))
        {


            if (BuildingStruct.comapareCosts(callingColony.resourcesOwned, purchciceCost) == true)
            {
                tileInfo tileScript = tileKVP.Key.GetComponent<tileInfo>();
                buildableGameObject buildable = new buildableGameObject
                {
                    buildCost = purchciceCost,
                    buildableObject = deathObject,
                    nameOfBuildable = thrbuildableScript.nameOfBuildable


                };

                bool succes = colonyMethoods.purchasableAction(myAI.gameObject, purchciceCost, tileKVP.Key, true);
                if (succes == true)
                {
                    GameObject objectToCheck =  tileScript.buildNewBuildable(buildable, callingColony);
                    amountBuilded += 1;

                    if(amountBuilded == amountTobuild)// waits until the last buildable is ready
                    {
                        buildableScript zogglisihs = objectToCheck.GetComponent<buildableScript>();
                        
                        trackingBuildable = zogglisihs;
                        zogglisihs.doneCreatingSelf += finsied;
                        
                    }

                }
                else
                {
                    Debug.Log("I FAILED <FMDSKFN");
                    finsied();
                }
                if (amountBuilded >= amountTobuild)
                {
                    break;
                }
              
               
                





            }
            else
            {
                finsied();
            }
           
        }
        void finsied()
        {
            
           
            trackingBuildable.doneCreatingSelf -= finsied;
            
            myAI.hasFreshDesiredbuildabe = false;
           
            finished = true;
        }
        
    }
}
public class massUseStrat : iActionStrat
{
    bool finishedAction = false;
    public bool canPerform => !complete;
    public bool complete => finishedAction;
    buildableScript.AIBuildableInfo.buildablePurposes performingType;
     buildableScript.buildableActions action;
     baseColonyAI colonyScript;
    
buildableScript.buildableActions actionToPerform;

    public massUseStrat(baseColonyAI thisColonyScript,buildableScript.AIBuildableInfo.buildablePurposes buildableType, buildableScript.buildableActions actionToPerform)
    {
       performingType = buildableType;
       action = actionToPerform;
       colonyScript = thisColonyScript;
    
       



    }
    public void Start()
    {
        buildableScript[] theOnesToDance = colonyScript.getTypeOfBuildableOwned(performingType);
        int amtPeforming = theOnesToDance.Length;
         int[] order = playerMouseInteractions.randomAssortment(amtPeforming);
         GameObject[] targets = colonyMethoods.bestTilesurrouning(colonyScript.gameObject,amtPeforming);
       
        for(int i = 0; i < amtPeforming; i++)
        {
            theOnesToDance[i].buildableAction(action, targets[order[i]]);
            theOnesToDance[i].finishedAction += hasFinishedAction;
            if (i >= targets.Length) 
            {
                i = 0;
            }
        }

    }

    void hasFinishedAction()
    {
        finishedAction = true;

    }
    
}


/// <summary>
/// ironically; mostly useless
/// </summary>
public class useStrat : iActionStrat// this suck but i dont think this will work in anyway, theses strats are initalized when COLONY start so there will never be a good way to feed info into it
{
    bool finishedAction = false;
    public bool canPerform => !complete;
    public bool complete => finishedAction;
    buildableScript[] performingBuildable;
     buildableScript.buildableActions action;
      GameObject[] targets;


    public useStrat(buildableScript[] buildables, buildableScript.buildableActions actionToPerform, GameObject[] targetsToActOn)
    {
       performingBuildable = buildables;
       action = actionToPerform;
       targets = targetsToActOn;
       Debug.LogWarning("loook here shisr: " + action+" le actions i thou "+ targetsToActOn.Length);



    }
    public void Start()
    {
         int[] order = playerMouseInteractions.randomAssortment(targets.Length);
         Debug.Log("length of action list targets is: " + targets.Length+ " and the objects it's applying to is: " + performingBuildable.Length);
        for(int i = 0; i < performingBuildable.Length; i++)
        {
            performingBuildable[i].buildableAction(action, targets[order[i]]);
            performingBuildable[i].finishedAction += hasFinishedAction;
            if (i >= targets.Length) 
            {
                i = 0;
            }
        }

    }

    void hasFinishedAction()
    {
        finishedAction = true;

    }
    
}


public class deleteStrat : iActionStrat
{
    bool builtTheThing;
    public bool canPerform => !complete;
    public bool complete => builtTheThing;
    colonyScript theColony;
    baseColonyAI colonyAi;
buildableScript theOnetoDelete;

    public deleteStrat(colonyScript colony,baseColonyAI superAI)
    {
        theColony = colony;
        colonyAi = superAI;
    }
    public void Start()
    {

        KeyValuePair<buildableScript.AIBuildableInfo.buildablePurposes, float> lowestValuedPurpose = new KeyValuePair<buildableScript.AIBuildableInfo.buildablePurposes, float>();

        foreach(KeyValuePair<buildableScript.AIBuildableInfo.buildablePurposes, float> purpose in colonyAi.valueOfBuildables)
        {
            if(purpose.Value < lowestValuedPurpose.Value)
            {
                lowestValuedPurpose = purpose;
            }
        }
        
        float strongest = 0;
        foreach(GameObject buildable in theColony.ownedBuildables)
        {
            buildableScript thisBuildableScript = buildable.GetComponent<buildableScript>();
            
            foreach(var purpose in thisBuildableScript.purposes)
            {
                if(purpose.purpose != lowestValuedPurpose.Key)
                {
                    continue;
                }
                
                if(purpose.strength >= strongest)
                {
                    strongest = purpose.strength;
                    theOnetoDelete =thisBuildableScript;

                }
            }
            
                
            

        }
        GameObject.Destroy(theOnetoDelete);
        

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


public class chooseBuildableStrat : iActionStrat
{
    bool builtTheThing;
    public bool canPerform => !complete;
    public bool complete => builtTheThing;
    baseColonyAI colonyAI;

    public chooseBuildableStrat(baseColonyAI sgfa)
    {
      colonyAI =sgfa;

            
        
    }
    public void Start()
    {
        

       
        
        
        colonyAI.desiredBuildable = bestBuildableToBuild();
        builtTheThing = true;
        
    
       
    }

    buildableGameObject bestBuildableToBuild()
    {
        bool isGoingMoneyBroke = colonyAI.thisColonyScript.resourcesOwned.moneyExpenses + colonyAI.thisColonyScript.totalIncome().moneyExpenses * 5 < 0;
        bool isGoingResourcebroke = colonyAI.thisColonyScript.resourcesOwned.resourceExpenses + colonyAI.thisColonyScript.totalIncome().resourceExpenses * 5 < 0;
        bool isGoingPeopleBroke = colonyAI.thisColonyScript.resourcesOwned.populationExpenses + colonyAI.thisColonyScript.totalIncome().populationExpenses * 5 < 0;
        
            
        
       var valuesOrdered =  colonyAI.valueOfBuildables.OrderByDescending(x=> x.Value).ToArray();

        for(int i = 0; i < valuesOrdered.Length;i++)// for every value
        {
           
            buildableGameObject[] allOfACategory = colonyAI.getTypeOfBuildableObject(valuesOrdered[i].Key);
            for(int k = 0; k < allOfACategory.Length;k++)
            {
               
                buildableScript currebuildablescript = allOfACategory[k].buildableObject.GetComponent<buildableScript>();
                BuildingStruct upkeepcosts =  currebuildablescript.upkeepCosts;
                 Debug.Log(valuesOrdered[i].Key +"LOOOOOOOOOOOOOOOOOOOOOOOOk" +" and the object is: "+ allOfACategory[k].buildableObject);
                if(isGoingMoneyBroke == true && upkeepcosts.moneyExpenses >0)
                {
                     Debug.Log(valuesOrdered[i].Key +"LOOOOOOOOOOOOOOOOOOOOOOOOk" +" and the object is: "+ allOfACategory[k].buildableObject+" and it failed moneywidse");
                    continue;
                }
                if(isGoingResourcebroke == true && upkeepcosts.resourceExpenses >0)
                {Debug.Log(valuesOrdered[i].Key +"LOOOOOOOOOOOOOOOOOOOOOOOOk" +" and the object is: "+ allOfACategory[k].buildableObject+" and it failed resourcesWise");
                    continue;
                }
                if(isGoingPeopleBroke == true && upkeepcosts.populationExpenses >0)
                {Debug.Log(valuesOrdered[i].Key +"LOOOOOOOOOOOOOOOOOOOOOOOOk" +" and the object is: "+ allOfACategory[k].buildableObject+" and it failed peoplewise");
                    continue;
                }
                Debug.Log("IM SETTING FRESH BUILDABLE TO TRUE");
                
                colonyAI.hasFreshDesiredbuildabe = true;


                return allOfACategory[k];
            }

        }
        Debug.Log("LOOOOOOOOOOOOOOOOOOOOOOOOk" +" im bout to null"+" length wasss"+ valuesOrdered.Length);
                
        return null;
    }

   

    
       
}

public class waitStrat : iActionStrat
{

    public bool canPerform => true;
    public bool complete{ get; private set; }


    CountDownTimer timer;

    public waitStrat(colonyScript colony, float duration, int testNum)
    {
        complete = false;
        Debug.Log(colony.gameObject.name);
        timer = new CountDownTimer(duration);
        timer.onTimerStart += () => complete = false;
        timer.onTimerEnd += () => colony.tempGoapTestNumber = testNum;
        timer.onTimerEnd += () => complete = true;

    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.tick(deltaTime);
}


}
