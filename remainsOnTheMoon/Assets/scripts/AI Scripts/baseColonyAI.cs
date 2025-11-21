using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class baseColonyAI : MonoBehaviour// high level decision maker for colony, does not directly control buildable but instead guides them
{
    int desiredSize;
    BuildingStruct desiredIncome;

    public buildableGameObject desiredBuildable;

    

    
    public event Action AITick;
    public GameObject theGameManager;
    public Dictionary<buildableScript.AIBuildableInfo.buildablePurposes, float> valueOfBuildables = new Dictionary<buildableScript.AIBuildableInfo.buildablePurposes, float>();//how much the ai will priorities the buildable



    public float aggression; // how much will the ai consider other colonies when making decisions



    public colonyScript thisColonyScript;


// all variables below are for goap, don't mess with them willy nilly
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
        setupBuildableAIValues();
        setupTimers();// useless
        setupBeliefs();
        setupActions();
        setupGoals();
        
    }
    /// <summary>
    /// assigns how much the ai will care about building this particular buildable, theses values should change as the game progress to reflect how important having that thing at that time is
    /// </summary>
    void setupBuildableAIValues()
    {
      
        foreach(buildableScript.AIBuildableInfo.buildablePurposes purposes in buildablesPurposesGrouped.buildablePurposeDictonary.Keys)  //assign all purposes the same value just in case
        {
            valueOfBuildables.Add(purposes,0.5f);
        }

        // manually assign values here, they should still drift from theses inital values though
        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.antiMissile] = 0.15f;
        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.defensive] = 0.20f;
        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.economy] = 0.50f;
        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.expansion] = 0.60f;
        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.offensive] = 0.25f;
        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive] = 0.30f;// missiles should be a prevent threat of the game, to like expand the metaphor and stuff

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

        factory.addBeliefs("is happy with size", () => thisColonyScript.allTilesOwned.Count > 2);
   
       
        
        factory.addBeliefs("satisfied with buildables", () => false); // ai can never be satiated
         factory.addBeliefs("has decided on buildable", () => desiredBuildable != null);
        factory.addBeliefs("has space to build", hasSpaceToBuild);

        bool hasSpaceToBuild()
        {
           
            foreach(GameObject tile in thisColonyScript.allTilesOwned)
            {
                tileInfo tileInfo = tile.GetComponent<tileInfo>();

                if(tileInfo.occupid ==false)
                {
                    return true;
                }
            }

            return false;
        }
         
       




    }
    
    void setupActions()
    {
        actions = new HashSet<agentAction>();



        
        
        

        actions.Add(new agentAction.Builder("make space")
        .WithStrat(new chooseBuildableStrat(this))
        .AddEffect(beliefs["has decided on buildable"])
        .Build());

        actions.Add(new agentAction.Builder("decide Buildable")
        .WithStrat(new chooseBuildableStrat(this))
        .AddEffect(beliefs["has decided on buildable"])
        .Build());
        
    
        

        actions.Add(new agentAction.Builder("buildBuildable")
        .WithStrat(new buildStrat(gameObject,desiredBuildable.buildableObject,desiredBuildable.buildCost,1,this))
        .addPreCondition(beliefs["has decided on buildable"])
        .addPreCondition(beliefs["has space to build"])
        .AddEffect(beliefs["satisfied with buildables"])
        .Build());
        
        



        actions.Add(new agentAction.Builder("settle new land")
        .WithStrat(new useStrat(
        getTypeOfBuildable(buildableScript.AIBuildableInfo.buildablePurposes.expansion),
        buildableScript.buildableActions.GenericAction,
        colonyMethoods.bestTilesurrouning(gameObject, getTypeOfBuildable(buildableScript.AIBuildableInfo.buildablePurposes.expansion).Length)
        )).AddEffect(beliefs["is happy with size"])
        .addPreCondition(beliefs["has Settlers"])
        .Build());



    }

    void setupGoals()
    {

        goals = new HashSet<AgentGoal>();
        

        
        goals.Add(new AgentGoal.Builder("make more buildables")
        .withPriority(0.25f)
        .withdesiredEffects(beliefs["satisfied with buildables"])
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
    public buildableScript[] getTypeOfBuildableOwned(buildableScript.AIBuildableInfo.buildablePurposes dog, float strengthRequired = 0)
    {
        List<buildableScript> allBuildables = new List<buildableScript>();
        List<buildableScript> selectedBuildables = new List<buildableScript>();
        foreach (GameObject currentBuildable in thisColonyScript.ownedBuildables)
        {
            buildableScript currentscript = currentBuildable.GetComponent<buildableScript>();
            foreach (buildableScript.AIBuildableInfo.biInfoStuct infoStuct in currentscript.purposes)
            {
                if (infoStuct.purpose == dog && infoStuct.strength < strengthRequired)
                {
                    selectedBuildables.Add(currentscript);
                    break;// if purpose is found then no need to check the rest

                }

            }


        }
        return selectedBuildables.ToArray();
        

    }
    /// <summary>
    /// returns the strongest first
    /// </summary>
    /// <param name="dog"></param>
    /// <returns></returns>
    public buildableScript[] getTypeOfBuildable(buildableScript.AIBuildableInfo.buildablePurposes dog)// NOT FINISHED NOT FINISHED FIX NOW NOW NOW
    {
        buildableGameObject[] allSelected = buildablesPurposesGrouped.buildablePurposeDictonary[dog].ToArray();
        if(allSelected ==null)
        {
            return null;
        }
        List<buildableScript> buildableScripts = new List<buildableScript>();
        foreach(buildableGameObject currentSelected in allSelected)
        {
            buildableScript buildableScript = currentSelected.buildableObject.GetComponent<buildableScript>();
        }
        return null;
        
       
    }
    public static buildableGameObject getBuildableGameObject(buildableScript buildableScript)
    {
        buildingCatagory[] idk = gameManagerScript.allCats;
       // List<buildableGameObject> idkv2 = new List<buildableGameObject>();
        foreach(buildingCatagory individualCat in idk)
        {
            foreach(buildableGameObject indvidualBuildable in individualCat.arrayOfBuildings)
            {
                if(buildableScript == indvidualBuildable.buildableObject)
                {
                    return indvidualBuildable;
                }
                
            }
            
        }
        return null;

       
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





    



