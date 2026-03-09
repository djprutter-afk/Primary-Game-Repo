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
            purposeWanted = colonyAI.desiredPurposesOfBuildable;
            
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

    
      
       
        
    Debug.LogWarning("no buildable found");

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
            Debug.Log("checking if null: buildCost is null? " + buildableGameObject.IsUnityNull());
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
            if(i == amountCanAfford)
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
    public float priority { get;  set; }

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