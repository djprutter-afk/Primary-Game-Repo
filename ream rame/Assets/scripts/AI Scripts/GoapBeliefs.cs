using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
// this is all made with tuorial: https://www.youtube.com/watch?v=T_sBYgP7_2k&t=613s
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
    void Start()
    {

    }
    void Update(float deltaTime)
    {

    }
    void Stop()
    {

    }
}

public class spendStrat : iActionStrat
{
    bool builtTheThing;
    public bool canPerform => !complete;
    public bool complete => builtTheThing;

    public spendStrat(colonyScript colony)
    {
        colony.resourcesOwned.moneyExpenses = -100;
        builtTheThing = true;
    }
}


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

public class GoapPlanner : IGoapPlanner
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
                newAvailableActions.Remove(action);

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