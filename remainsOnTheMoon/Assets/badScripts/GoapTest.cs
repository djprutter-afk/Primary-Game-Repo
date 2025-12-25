using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapAITestSetup : MonoBehaviour
{
    [SerializeField] baseColonyAI agent; // assign your existing agent in Inspector

    beliefFactory factory;
    GoapPlanner planner;

    Dictionary<string, agentBelief> beliefs = new();
    HashSet<agentAction> actions = new();
    HashSet<AgentGoal> goals = new();

    // Dummy world state
    bool hasSupplies = false;
    bool houseClean = false;
    bool guestsInvited = false;
    bool foodCooked = false;
    bool partyStarted = false;

    void Start()
    {
        if (agent == null)
        {
            Debug.LogError("Please assign your baseColonyAI reference to GoapAITestSetup.");
            return;
        }

        // Initialize belief system & planner
        factory = new beliefFactory(agent, beliefs);
        planner = new GoapPlanner();

        // --- Step 1: Create beliefs ---
        factory.addBeliefs("HasSupplies", () => hasSupplies);
        factory.addBeliefs("HouseClean", () => houseClean);
        factory.addBeliefs("GuestsInvited", () => guestsInvited);
        factory.addBeliefs("FoodCooked", () => foodCooked);
        factory.addBeliefs("PartyStarted", () => partyStarted);

        // --- Step 2: Define test actions ---
        actions.Add(new agentAction.Builder("GatherSupplies")
            .WithStrat(new TimedTestStrat(() => hasSupplies = true))
            .AddEffect(beliefs["HasSupplies"])
            .Build());

        actions.Add(new agentAction.Builder("CleanHouse")
            .WithStrat(new TimedTestStrat(() => houseClean = true))
            .AddEffect(beliefs["HouseClean"])
            .Build());

        actions.Add(new agentAction.Builder("InviteGuests")
            .WithStrat(new TimedTestStrat(() => guestsInvited = true))
            .AddEffect(beliefs["GuestsInvited"])
            .Build());

        actions.Add(new agentAction.Builder("CookFood")
            .WithStrat(new TimedTestStrat(() => foodCooked = true))
            .AddEffect(beliefs["FoodCooked"])
            .Build());

        actions.Add(new agentAction.Builder("StartParty")
            .WithStrat(new TimedTestStrat(() => partyStarted = true))
            .addPreCondition(beliefs["HasSupplies"])
            .addPreCondition(beliefs["HouseClean"])
            .addPreCondition(beliefs["GuestsInvited"])
            .addPreCondition(beliefs["FoodCooked"])
            .AddEffect(beliefs["PartyStarted"])
            .Build());

        // Inject actions into the agent
        agent.actions = actions;

        // --- Step 3: Create a goal ---
        goals.Add(new AgentGoal.Builder("ThrowParty")
            .withPriority(1)
            .withdesiredEffects(beliefs["PartyStarted"])
            .Build());

        // --- Step 4: Plan ---
        var plan = planner.Plan(agent, goals);

        if (plan == null)
        {
            Debug.LogError("No plan found!");
            return;
        }

        Debug.Log($"✅ PLAN FOUND: {plan.AgentGoal.Name}");
        foreach (var act in plan.Actions)
        {
            Debug.Log($" → {act.name}");
        }

        // --- Step 5: Execute Plan ---
        StartCoroutine(RunPlan(plan));
    }

    IEnumerator RunPlan(ActionPlan plan)
    {
        Debug.Log("🏁 EXECUTING PLAN...");
        foreach (var act in plan.Actions.Reverse())
        {
            Debug.Log($"Performing action: {act.name}");
            act.Start();
            float elapsed = 0f;

            while (!act.complete)
            {
                act.Update(Time.deltaTime);
                elapsed += Time.deltaTime;
                if (elapsed > 3f) break; // just safety timeout
                yield return null;
            }

            act.stop();
            Debug.Log($"✅ Action complete: {act.name}");
        }

        Debug.Log("🎉 PLAN COMPLETE!");
    }
}

/// <summary>
/// Simple “wait one second and mark done” strategy.
/// </summary>
public class TimedTestStrat : iActionStrat
{
    bool done = false;
    float timer = 0f;
    readonly Action onComplete;

    public TimedTestStrat(Action onComplete)
    {
        this.onComplete = onComplete;
    }

    public bool canPerform => !done;
    public bool complete => done;

    public void Start() { timer = 0f; done = false; }

    public void Update(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= 1f)
        {
            done = true;
            onComplete?.Invoke();
        }
    }

    public void Stop() { }
}
