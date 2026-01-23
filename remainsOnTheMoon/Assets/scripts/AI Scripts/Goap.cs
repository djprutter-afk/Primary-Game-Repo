using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using UnityEngine.Tilemaps;
using Unity.Collections;
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
  
    

    public void addLocationBelief(string key, float distance, Vector3 locationCOndition)
    {
        beliefs.Add(key, new agentBelief.Builder(key)
        .withCondition(() => InRangeOf(locationCOndition, distance))
        .withLocation(() => locationCOndition)
        .Build());
        
    }
    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position, pos) < range;
}
public class agentBelief
{
    public string Name { get; }
    Func<bool> condition = () => false;
    Func<Vector3> observedLocation = () => Vector3.zero;

    public Vector3 Location => observedLocation();
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
        public Builder withLocation(Func<Vector3> observedLocations)
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





public class chooseBuildableecoStrat : iActionStrat
{

    
    baseColonyAI theAi;

    public bool canPerform => !complete;
    bool hasDecided = false;
    public bool complete => hasDecided;
    public chooseBuildableecoStrat(baseColonyAI leAI)
    {
        theAi = leAI;
        
    }
    public void Start()
    {
        buildableGameObject[] allOfACategory = theAi.getTypeOfBuildableObject(buildableScript.AIBuildableInfo.buildablePurposes.economy);
        buildableGameObject candidate = new();
        float candidateGoodness = -9999999999999;
       TriValueStruct income = theAi.thisColonyScript.totalIncome();
       
        foreach(buildableGameObject buildableEco in allOfACategory)
        {
           // BuildingStruct buildeficit = theAi.thisColonyScript.resourcesOwned.subtract(buildableEco.buildCost.multiply(5));
           // if(BuildingStruct.comapareCosts(buildeficit))
            //{
                //continue;
                
            //}
            buildableScript buildable = buildableEco.buildableObject.GetComponent<buildableScript>();
            TriValueStruct builableincome = buildable.upkeepCosts;
            
            TriValueStruct incomeChange = income.subtract(builableincome);
            TriValueStruct changePercent = incomeChange.divide(income);
            float goodness = (1- changePercent.moneyValue) + (1-changePercent.resourceValue) + (1-changePercent.populationValue);
            Debug.Log("the goodness of buildabe is: " + goodness + " and buildable name is: " +buildable.name);
            if(goodness >= candidateGoodness)
            {
                candidate = buildableEco;
                candidateGoodness = goodness;
            }
            
        }
        if(candidate == null)
        {
            theAi.desiredBuildable =null;
            theAi.hasFreshDesiredbuildabe = false;
            hasDecided = true;
            return;
        }
        
        theAi.desiredBuildable =candidate;
        theAi.hasFreshDesiredbuildabe = true;
        hasDecided = true;
        
    }




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
            float distance = Vector3.Distance(targetPos.transform.position, currentTile.transform.position);
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
        bool isGoingMoneyBroke = colonyAI.thisColonyScript.resourcesOwned.moneyValue + colonyAI.thisColonyScript.totalIncome().moneyValue * 5 < 0;
        bool isGoingResourcebroke = colonyAI.thisColonyScript.resourcesOwned.resourceValue + colonyAI.thisColonyScript.totalIncome().resourceValue * 5 < 0;
        bool isGoingPeopleBroke = colonyAI.thisColonyScript.resourcesOwned.populationValue + colonyAI.thisColonyScript.totalIncome().populationValue * 5 < 0;
        
            
        
       var valuesOrdered =  colonyAI.valueOfBuildables.OrderByDescending(x=> x.Value).ToArray();

        for(int i = 0; i < valuesOrdered.Length;i++)// for every value
        {
           
            buildableGameObject[] allOfACategory = colonyAI.getTypeOfBuildableObject(valuesOrdered[i].Key);
            for(int k = 0; k < allOfACategory.Length;k++)
            {
               
                buildableScript currebuildablescript = allOfACategory[k].buildableObject.GetComponent<buildableScript>();
                TriValueStruct upkeepcosts =  currebuildablescript.upkeepCosts;
                 
                if(isGoingMoneyBroke == true && upkeepcosts.moneyValue >0)
                {
                   
                    continue;
                }
                if(isGoingResourcebroke == true && upkeepcosts.resourceValue >0)
                {
                    continue;
                }
                if(isGoingPeopleBroke == true && upkeepcosts.populationValue >0)
                {
                    continue;
                }
                Debug.Log("IM SETTING FRESH BUILDABLE TO TRUE");
                
                colonyAI.hasFreshDesiredbuildabe = true;


                return allOfACategory[k];
            }

        }
         colonyAI.hasFreshDesiredbuildabe = false;
   
                
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