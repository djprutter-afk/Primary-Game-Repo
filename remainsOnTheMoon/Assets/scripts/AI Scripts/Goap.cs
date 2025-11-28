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
/// <summary>
/// should be removed
/// </summary>




public class waitTickStrat : iActionStrat
{

    int ticksRemaining;

    public bool canPerform => !complete;
    public bool complete => ticksRemaining == 0;
    public waitTickStrat(int ticks)
    {
        ticksRemaining = ticks;
        gameManagerScript.GameTick += this.tick;


    }
    void tick()
    {
        ticksRemaining--;

    }
    public void Start()
    {
        
    }




}


public class buildStrat : iActionStrat
{
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPos">the tile position where the strat will try to build the buildables around</param>
    /// <param name="buildableToBuild"></param>
    /// <param name="buildingCost"></param>
    /// <param name="CallingColony"></param>
    public buildStrat(GameObject sdfsdf, GameObject buildableToBuild, BuildingStruct BuildCost, int dsf, baseColonyAI fdg)
    {
        amountTobuild = dsf;
        targetPos = sdfsdf;
        callingColony = fdg.thisColonyScript;
        purchciceCost = BuildCost;
         thrbuildableScript = buildableToBuild.GetComponent<buildableScript>();
        deathObject = buildableToBuild;
        myAI = fdg;





    }
    public void Start()
    {
        
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
                    tileScript.buildNewBuildable(buildable, callingColony);
                    amountBuilded += 1;
                }
                if (amountBuilded >= amountTobuild)
                {
                    break;
                }
                myAI.desiredBuildable = null;
                





            }

        }
        finished = true;
    }
}


/// <summary>
/// ironically; mostly useless
/// </summary>
public class useStrat : iActionStrat
{
    bool finishedAction = false;
    public bool canPerform => !complete;
    public bool complete => finishedAction;
    buildableScript[] performingBuildable;
     buildableScript.buildableActions action;
      GameObject[] targets;

    /// <summary>
    /// perform action at mass
    /// </summary>
    /// <param name="performingBuildable"></param>
    /// <param name="action"></param>
    /// <param name="target"></param>
    public useStrat(buildableScript[] rthb, buildableScript.buildableActions hsejh, GameObject[] rtjrsj)
    {
       performingBuildable = rthb;
       action = hsejh;
       targets = rtjrsj;



    }
    public void Start()
    {
         int[] order = playerMouseInteractions.randomAssortment(targets.Length);
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
    /// <summary>
    /// peform action on only one, single, lonely buildable
    /// </summary>
    /// <param name="performingBuildable"></param>
    /// <param name="action"></param>
    /// <param name="target"></param>
    
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
         var valuesOrdered =  colonyAI.valueOfBuildables.OrderByDescending(x=> x.Value).ToArray();
       if(valuesOrdered.Length <= 0)
        {
            Debug.LogError("FAIL VALUES ORDER WAS " + valuesOrdered.Length);
            return;
        }
        buildableGameObject[] allOfACategory = colonyAI.getTypeOfBuildableObject(valuesOrdered[0].Key);
        if(allOfACategory.Length <= 0)
        {
             Debug.LogError("FAIL CAT WAS " + allOfACategory.Length);
            return;
        }
        colonyAI.desiredBuildable = allOfACategory[0];
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