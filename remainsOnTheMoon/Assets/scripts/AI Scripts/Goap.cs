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
public class beliefFactory
{

    readonly baseColonyAI agent;
    readonly Dictionary<string, agentBelief> beliefs = new Dictionary<string, agentBelief>();

    public beliefFactory(baseColonyAI agent, Dictionary<string, agentBelief> beliefs)
    {
        this.agent = agent;
        this.beliefs = beliefs;
    }


    public void addBeliefs(string key, Func<bool> condition)
    {
        beliefs.Add(key, new agentBelief.Builder(key).withCondition(condition).Build());
    }
  
    

    public void addLocationBelief(string key, float distance, UnityEngine.Vector3 locationCOndition)
    {
        beliefs.Add(key, new agentBelief.Builder(key)
        .withCondition(() => InRangeOf(locationCOndition, distance))
        .withLocation(() => locationCOndition)
        .Build());
        
    }
    bool InRangeOf(UnityEngine.Vector3 pos, float range) => UnityEngine.Vector3.Distance(agent.transform.position, pos) < range;
}
public class agentBelief
{
    public string Name { get; }
    Func<bool> condition = () => false;
    Func<UnityEngine.Vector3> observedLocation = () => UnityEngine.Vector3.zero;

    public UnityEngine.Vector3 Location => observedLocation();
    agentBelief(string name)
    {
        Name = name;
    }

    public bool Evaluate() => condition();
    public class Builder
    {
        readonly agentBelief belief;
        public Builder(string name)
        {
            belief = new agentBelief(name);
        }
        public Builder withCondition(Func<bool> condition)
        {
            belief.condition = condition;
            return this;
        }
        public Builder withLocation(Func<UnityEngine.Vector3> observedLocations)
        {
            belief.observedLocation = observedLocations;
            return this;
        }
        public agentBelief Build()
        {
            return belief;
        }


    }
}


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
public class decideTimeTowait : iActionStrat
{

    int? ticksRemaining;
    baseColonyAI theAI;
    bool isFin = false;
    public bool canPerform => !complete;
    public bool complete => isFin;
    public decideTimeTowait(baseColonyAI THEAI)
    {
        theAI = THEAI;
         
        
    }
    void tick()
    {
        ticksRemaining--;
        if(ticksRemaining<=0)
        {
            finishedTicking();
        }

    }
    void finishedTicking()
    {
        theAI.hasntWaited = false;
        gameManagerScript.GameTick -= this.tick; 
        isFin=true;
    }
    public void Start()
    {
        theAI.hasntWaited = false;
        for(int i=0; i< 15;i++)
        {
            bool canAfford = TriValueStruct.comapareCosts(theAI.thisColonyScript.resourcesOwned.addition(theAI.thisColonyScript.totalIncome().multiply(i)), theAI.desiredBuildable.buildCost);
            if(canAfford == true)
            {
                ticksRemaining = i +1;
                break;
            }
            //Debug.Log(i+" " + theAI.thisColonyScript.resourcesOwned.addition(theAI.thisColonyScript.totalIncome().multiply(i)));
        }
        if(ticksRemaining==null)
        {
            ticksRemaining = 0;

        }
      



       gameManagerScript.GameTick += this.tick; 
    }




}

public class buildStrat : iActionStrat
{
    buildableScript trackingBuildable; 
    bool finished = false;
    TriValueStruct purchciceCost;
    colonyScript callingColony;
    GameObject deathObject;
    buildableScript thrbuildableScript;

    public bool canPerform => complete!;
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
    { 
        myAI.hasntWaited = true;
        
        thrbuildableScript =myAI.desiredBuildable.buildableObject.GetComponent<buildableScript>();
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
            float distance = UnityEngine.Vector3.Distance(targetPos.transform.position, currentTile.transform.position);
            tileDic.Add(currentTile, distance);
        }

        int amountBuilded = 0;

        foreach (KeyValuePair<GameObject, float> tileKVP in tileDic.OrderBy(x => x.Value))
        {


            if (TriValueStruct.comapareCosts(callingColony.resourcesOwned.addition(callingColony.totalIncome().multiply(5)), purchciceCost) == true)
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
                     GameObject objectToCheck =  tileScript.buildNewBuildable(buildable, callingColony);
                    amountBuilded += 1;

                    if(amountBuilded == amountTobuild)// waits until the last buildable is ready
                    {
                        buildableScript zogglisihs = objectToCheck.GetComponent<buildableScript>();
                        
                        trackingBuildable = zogglisihs;
                        zogglisihs.doneCreatingSelf += finsied;
                        
                    }
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
            
           if(trackingBuildable != null)
            {
                trackingBuildable.doneCreatingSelf -= finsied;
            }
            
            
            myAI.hasFreshDesiredbuildabe = false;
           
            finished = true;
        }
        
    }
}



public class buildableUseStrat : iActionStrat
{
    bool finishedAction = false;
    public bool canPerform => !complete;
    public bool complete => finishedAction;
    baseColonyAI colonyAI;
buildableScript.AIBuildableInfo.buildablePurposes buildablePurposeNeeded;
buildableScript.buildableActions actionToUse;
Func<GameObject,int,UnityEngine.Vector3[]> postionDecider;
int positionsToFind;


    public buildableUseStrat(baseColonyAI ColonyAI, buildableScript.AIBuildableInfo.buildablePurposes BuildablePurposeNeeded,buildableScript.buildableActions actionToUse, Func<GameObject,int,UnityEngine.Vector3[]> PositionDecider,int PositionsTofind =1)
    {
        colonyAI = ColonyAI;
        buildablePurposeNeeded = BuildablePurposeNeeded;
        this.actionToUse = actionToUse;
        postionDecider = PositionDecider;
        positionsToFind = PositionsTofind;
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
       
        UnityEngine.Vector3[] positionsToUse = postionDecider(colonyAI.gameObject,positionsToFind);
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
                GameObject[] tiles = hitColliders.Select(x=>x.gameObject).Where(x=>x.TryGetComponent(out tileInfo Tile) ==true).ToArray();

                if(tiles.Length >targetsPerPosition)
                {

                    tileClosestToPosition.Add(position,tiles.Take(targetsPerPosition).ToArray());
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
                if(currentValueUsed + bestBuildable.Value < valueNeed)
                {
                   
                    bestBuildable.Key.buildableAction(actionToUse,position.Value[i]);
                    currentValueUsed += bestBuildable.Value;
                    bestBuildable.Key.finishedAction += hasFinishedAction;

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

/// <summary>
/// for testing only, dont actually used
/// </summary>
/*
public class begStrat : iActionStrat
{
    bool builtTheThing;
    public bool canPerform => !complete;
    public bool complete => builtTheThing;

    public begStrat(colonyScript colony)
    {
        colony.resourcesOwned.moneyExpenses = 100;
        builtTheThing = true;
    }
}
*/
/// <summary>
/// destroys the least usefull buildable
/// </summary>
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
        buildableScript.AIBuildableInfo.buildablePurposes[] purposeWanted = colonyAI.desiredPurposesOfBuildable;

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

    
      
       
        


        return null;

        
        
            
       
     
    }
    float evaluatePurposeToColony(buildableScript.AIBuildableInfo.biInfoStuct purpose, baseColonyAI colonyAI)
    {
        return colonyAI.valueOfBuildables[purpose.purpose] * purpose.strength;
    }  
    float totalBurdenOfBuildable(buildableGameObject buildableGameObject,TriValueStruct colonyIncome)
    {
       
        buildableScript buildable = buildableGameObject.buildableObject.GetComponent<buildableScript>();
        TriValueStruct buildableUpkeep = buildable.upkeepCosts;
        float totalBurden = costEvaluation(buildableUpkeep,colonyIncome) + costEvaluation(buildableGameObject.buildCost,colonyIncome)/2.5f; // buildableupkeep should be weighted more cause ai will have to live with it longer
        return totalBurden;
    }
    
    float costEvaluation(TriValueStruct changeAmount,TriValueStruct initalValue)
    {
       float doubleNegativeBurden = -2;
       
       float dNBMoney = 1;
       if(changeAmount.moneyValue<0 && initalValue.moneyValue<0){dNBMoney = doubleNegativeBurden;}

       float dNBResource = 1;
       if(changeAmount.resourceValue<0 && initalValue.resourceValue<0){dNBResource = doubleNegativeBurden;}

       float dNBPopulation =1;
       if(changeAmount.populationValue<0 && initalValue.populationValue<0){dNBPopulation = doubleNegativeBurden;}
       TriValueStruct incomeChange = initalValue.subtract(changeAmount);
       TriValueStruct safeInitial = initalValue.addition(TriValueStruct.one.multiply(0.01f));

       TriValueStruct changePercent = incomeChange.divide(safeInitial);
       return (1- changePercent.moneyValue) *dNBMoney + (1-changePercent.resourceValue)*dNBResource + (1-changePercent.populationValue)*dNBPopulation;
      
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

public class agentAction
{
    public string name { get; }
    public float cost { get; private set; }
    public HashSet<agentBelief> preconditions { get; } = new();
    public HashSet<agentBelief> effects { get; } = new();

    iActionStrat strategy;
    public bool complete => strategy.complete;
    agentAction(string Name)
    {
        name = Name;
    }

    public void Start() => strategy.Start();
    public void Update(float deltaTime)
    {
        if (strategy.canPerform == true)
        {
            strategy.Update(deltaTime);
        }
        if (strategy.complete == false)
        {
            return;
        }
        foreach (var effect in effects)
        {
            effect.Evaluate();
        }
    }
    public void stop() => strategy.Stop();

    public class Builder
    {
        readonly agentAction action;

        public Builder(string name)
        {
            action = new agentAction(name)
            {
                cost = 1
            };
        }

        public Builder WithCost(float cost)
        {
            action.cost = cost;
            return this;
        }
        public Builder WithStrat(iActionStrat strategy)
        {
            action.strategy = strategy;
            return this;
        }
        public Builder addPreCondition(agentBelief precondition)
        {
            action.preconditions.Add(precondition);
            return this;
        }

        public Builder AddEffect(agentBelief effect)
        {
            action.effects.Add(effect);
            return this;
        }
        public agentAction Build()
        {
            return action;
        }
    }
}

public class AgentGoal
{


    public string Name { get; }
    public float priority { get; private set; }

    public HashSet<agentBelief> DesiredEffects { get; } = new();
    AgentGoal(string name)
    {
        Name = name;

    }

    public class Builder
    {
        readonly AgentGoal goal;
        public Builder(string name)
        {
            goal = new AgentGoal(name);
        }
        public Builder withPriority(float priority)
        {
            goal.priority = priority;
            return this;
        }
        public Builder withdesiredEffects(agentBelief effet)
        {
            goal.DesiredEffects.Add(effet);
            return this;
        }
        public AgentGoal Build()
        {
            return goal;
        }
    }





}
public interface IGoapPlanner
{
    ActionPlan Plan(baseColonyAI agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);

}

public class GoapPlanner : IGoapPlanner // this should be multithreaded at some point TODO
{
    public ActionPlan Plan(baseColonyAI agent, HashSet<AgentGoal> goals, AgentGoal mostrecentGoal = null)
    {
        List<AgentGoal> orderGoals = goals
        .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
        .OrderByDescending(g => g == mostrecentGoal ? g.priority - 0.01 : g.priority)
        .ToList();

        foreach (var goal in orderGoals)
        {
            Node goalNode = new Node(null, null, goal.DesiredEffects, 0);

            if (findPath(goalNode, agent.actions) == true)
            {
                if (goalNode.isLeafDead)
                {
                    continue;
                }
                Stack<agentAction> actionStack = new Stack<agentAction>();
                while (goalNode.Leaves.Count > 0)
                {
                    var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    goalNode = cheapestLeaf;
                    actionStack.Push(cheapestLeaf.Action);
                }
                return new ActionPlan(goal, actionStack, goalNode.Cost);
            }
        }

        Debug.LogWarning("no plan found");
        return null;
    }
    
    bool findPath(Node parent, HashSet<agentAction> actions)
    {
        if(actions == null)
        {
            return false;
        }
        var orderedActions = actions.OrderBy(a => a.cost);
        foreach (var action in orderedActions)
        {
            var requiredEffects = parent.requiredEffects;
            requiredEffects.RemoveWhere(b => b.Evaluate());

            if (requiredEffects.Count == 0)
            {
                return true;
            }
            if (action.effects.Any(requiredEffects.Contains))
            {
                var newRequiredEffects = new HashSet<agentBelief>(requiredEffects); ;
                newRequiredEffects.ExceptWith(action.effects);
                newRequiredEffects.UnionWith(action.preconditions);

                var newAvailableActions = new HashSet<agentAction>(actions);
                //newAvailableActions.Remove(action);  // have no idea but removing this line fixed everything, keeping it just in case

                var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.cost);

                if (findPath(newNode, newAvailableActions))
                {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.preconditions);
                }

                if (newRequiredEffects.Count == 0)
                {
                    return true;
                }
            }
        }
        return false;
        
    }
}
public class Node
{
    

    public Node Parent { get; }
    public agentAction Action { get; }
    public HashSet<agentBelief> requiredEffects { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }
    public bool isLeafDead => Leaves.Count == 0 && Action == null;
    public Node(Node parent, agentAction action, HashSet<agentBelief> effects, float cost)
    {
        Parent = parent;
        Action = action;
        requiredEffects = new HashSet<agentBelief>(effects);
        Leaves = new List<Node>();
        Cost = cost;
    }



}
public class ActionPlan
{
    public AgentGoal AgentGoal { get; }
    public Stack<agentAction> Actions { get; }
    public float TotalCost;
    public ActionPlan(AgentGoal goal, Stack<agentAction> actions, float totalCost)
    {
        AgentGoal = goal;
        Actions = actions;
        TotalCost = totalCost;
    }
}