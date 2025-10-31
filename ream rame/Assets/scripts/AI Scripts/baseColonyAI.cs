using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class baseColonyAI : MonoBehaviour// high level decision maker for colony, does not directly control buildables but instead guides them
{
    

    
    public event Action AITick;
    public GameObject theGameManager;




    public float aggression; // how much will the ai consider other colonies when making decisions



    colonyScript thisColonyScript;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionplan;
    public agentAction currentAction;

    public Dictionary<string, agentBelief> beliefs;
    public HashSet<agentAction> actions;
    public HashSet<AgentGoal> goals;
    CountDownTimer statsTimer;
    IGoapPlanner goapPlanner;

    


    void Awake()
    {

        thisColonyScript = GetComponent<colonyScript>();

        goapPlanner = new GoapPlanner();
    }
    void Start()
    {
        setupTimers();// useless
        setupBeliefs();
        setupActions();
        setupGoals();
    }

    void setupTimers()
    {
        statsTimer = new CountDownTimer(2f);
        statsTimer.onTimerEnd += () =>
        {
            updateStats();
            statsTimer.Start();
        };
        statsTimer.Start();

    }
  
    void updateStats()
    {

    }
    
    void setupBeliefs()
    {
        beliefs = new Dictionary<string, agentBelief>();
        beliefFactory factory = new beliefFactory(this, beliefs);

        factory.addBeliefs("Nothing", () => false);

        factory.addBeliefs("is going money broke", () => thisColonyScript.resourcesOwned.moneyExpenses - thisColonyScript.totalIncome().moneyExpenses * 2 < 0);
        
      //  factory.addBeliefs
       




    }
    
    void setupActions()
    {
        actions = new HashSet<agentAction>();

       

       //actions.Add(new agentAction.Builder("settle new land")
        //.WithStrat(new useStrat())



    }

    void setupGoals()
    {

        goals = new HashSet<AgentGoal>();
        /*
        goals.Add(new AgentGoal.Builder("Increase wealth")
        .withPriority(0.8f)
        .withdesiredEffects(beliefs["number1"])
        .Build());
        */

        
        goals.Add(new AgentGoal.Builder("expandSize")
        .withPriority(0.25f)
        .withdesiredEffects(beliefs["isMoneyRich"])
        .Build());
       
        

    }
    






    void colonyAiTick()
    {
        
        


        AITick?.Invoke();
        float randomInterval = UnityEngine.Random.Range(0.1f, 0.1f);// 100 to 200 milisecond delay
        Invoke(nameof(colonyAiTick), 3f + randomInterval);


    }
    void Update()
    {
        
        //statsTimer.tick(Time.deltaTime);
        if (currentAction == null)
        {
            Debug.Log("calculating any potential new plan");
            calculatePlan();
            if (actionplan != null && actionplan.Actions.Count > 0)
            {
                currentGoal = actionplan.AgentGoal;
                Debug.Log(thisColonyScript.tempGoapTestNumber);
                Debug.Log($"Goal: {currentGoal.Name} with {actionplan.Actions.Count} actions in plan");
                currentAction = actionplan.Actions.Pop();
                Debug.Log($"Popped action {currentAction.name}");
                if(currentAction.preconditions.All(b => b.Evaluate()))
                {
                    currentAction.Start();
                }
                else
                {
                    Debug.Log("preconditions are not met, clearing current action and goal");
                    currentAction = null;
                    currentGoal = null;
                }
            }
        }
        
        if (actionplan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);
            if (currentAction.complete)
            {
                Debug.Log($"{currentAction.name} complete");
                currentAction.stop();
                currentAction = null;

                if(actionplan.Actions.Count == 0)
                {
                    Debug.Log("plan complete");
                    
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }


    }
    void calculatePlan()
    {
        var priortyLevel = currentGoal?.priority ?? 0;
        HashSet<AgentGoal> goalsToCheck = goals;

        if (currentGoal != null)
        {
            Debug.Log("current goal exists, chekcing goal with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.priority > priortyLevel));
        }

        var potentialPlan = goapPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null)
        {
            actionplan = potentialPlan;
        }
    }

}
public class CountDownTimer
{
    public event Action onTimerStart;
    public event Action onTimerEnd;
    bool isCounting;
    float timeToWait;
    float timeLeft;
    
    public CountDownTimer(float timeAmt)
    {
        timeToWait = timeAmt;


    }
    public void Start()
    {
        
        isCounting = true;
        timeLeft = timeToWait;

        onTimerStart?.Invoke();
    }
    public void Stop()
    {
        isCounting = false;
    }

    public void tick(float timeElapsed)
    {
        Debug.LogWarning("time tick" + timeElapsed + " time left: " + timeLeft);

        if (isCounting == false)
        {
            return;
        }

        
        timeLeft -= timeElapsed;
        if (timeLeft <= 0)
        {
            timerEnd();
        }
    }
  
    
    void timerEnd()
    {
        Debug.Log(timeToWait + " FDF " + timeLeft);
        onTimerEnd?.Invoke();

        isCounting = false;
        
    }
}





    



