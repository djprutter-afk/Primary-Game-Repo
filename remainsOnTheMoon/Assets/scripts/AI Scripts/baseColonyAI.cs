 using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
/// <summary>
/// putting this here so i dont forget, money you should be able to go into debt but you cant go into debt for resource or population
/// </summary>
public class baseColonyAI : MonoBehaviour// high level decision maker for colony, does not directly control buildable but instead guides them
{
   
   



    

    
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
    

         
   public class otherColonyInfo
    {
        public float threatLevel;
        public float friendliness = 0.5f;
        public colonyScript colony;

        public void evaluateThreatLevel(colonyScript ownerColony,float selfMilitaryStrength)
        {
           
             Vector3 GetAveragePosition()
            {
                Vector3 averagePosition = Vector3.zero;
                foreach(GameObject tile in ownerColony.allTilesOwned)
                {
                    averagePosition += tile.transform.position;
                }
                return averagePosition /= ownerColony.allTilesOwned.Count;
            }   
                
        
            Vector3 colonyCenter = GetAveragePosition();
            float totalMilitaryValue =0;

           foreach(GameObject indivdualBuildable in colony.ownedBuildables)
            {
                buildableScript indivdualBuildableScript = indivdualBuildable.GetComponent<buildableScript>();
                foreach (buildableScript.AIBuildableInfo.biInfoStuct infoStuct in indivdualBuildableScript.purposes)
                {
                   if (infoStuct.purpose == buildableScript.AIBuildableInfo.buildablePurposes.offensive)
                   {
                       float distanceToCenter = Vector3.Distance(colonyCenter,indivdualBuildable.transform.position);// entire moon is 2 units across
                       
                       totalMilitaryValue += infoStuct.strength * (2-distanceToCenter);
                      

                   }

               }
            }
            threatLevel = totalMilitaryValue;


        }
        
      
    }

public List<otherColonyInfo> otherColonyInfos = new List<otherColonyInfo>();
    


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
        setupJudgementSystem();
        updateValues();
   
    

        
        gameManagerScript.GameTick += colonyAiTick;
        
    }
    void setupJudgementSystem()
    {
        colonyScript[] allColonies = transform.parent.GetComponent<gameSetup1>().allColonieScripts.ToArray();
        foreach(colonyScript otherColony in allColonies)
        {
            if(otherColony == thisColonyScript)
            {
                continue;
            }
            otherColonyInfos.Add(new otherColonyInfo{colony =otherColony});
        }
        
    }
    /// <summary>
    /// assigns how much the ai will care about building this particular buildable, theses values should change as the game progress to reflect how important having that thing at that time is
    /// </summary>
    void setupBuildableAIValues()
    {
      
        foreach(buildableScript.AIBuildableInfo.buildablePurposes purposes in buildablesPurposesGrouped.buildablePurposeDictonary.Keys)  //assign all purposes the same value just in case
        {
            valueOfBuildables.Add(purposes,0.01f);
        }

        // manually assign values here, they should still drift from theses inital values though
        //valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.antiMissile] = 0.15f;
        //valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.defensive] = 0.20f;
        //valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.economy] = 0.30f;
       // valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.expansion] = 0.60f;
        //valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.offensive] = 0.35f;
        //valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive] = 0.30f;// missiles should be a prevelent threat of the game

    }

    void setupTimers()
    {
        InvokeRepeating(nameof(updateValues),0f,5);
        /*
        statsTimer = new CountDownTimer(2f);
        statsTimer.onTimerEnd += () =>
        {
            Debug.LogError("about to update values");
           
            statsTimer.Start();
        };
        statsTimer.Start();
        */
    }
    
  
    void updateValues()
    {
        updateFear();
        updateGoalPressure();
    
 
    }

    void updateFear()
    {
           
        Vector3 GetAveragePosition()
        {
            Vector3 averagePosition = Vector3.zero;
            foreach(GameObject tile in thisColonyScript.allTilesOwned)
            {
                averagePosition += tile.transform.position;
            }
            return averagePosition /= thisColonyScript.allTilesOwned.Count;
        }   

        Vector3 colonyCenter = GetAveragePosition();
         float totalMilitaryOfSelf = 0f;
            foreach(GameObject indivdualBuildable in thisColonyScript.ownedBuildables)
            {
                 buildableScript indivdualBuildableScript = indivdualBuildable.GetComponent<buildableScript>();
                foreach (buildableScript.AIBuildableInfo.biInfoStuct infoStuct in indivdualBuildableScript.purposes)
                {
                   if (infoStuct.purpose == buildableScript.AIBuildableInfo.buildablePurposes.offensive)
                   {
                       float distanceToCenter = Vector3.Distance(colonyCenter,indivdualBuildable.transform.position);// entire moon is 2 units across
                       
                       totalMilitaryOfSelf += infoStuct.strength * (2-distanceToCenter);
                      

                   }

               }
            }
        float totalFear = 0f;
        foreach(var colonyInfo in otherColonyInfos)
        {
            colonyInfo.evaluateThreatLevel(thisColonyScript,totalMilitaryOfSelf);
            totalFear+= colonyInfo.threatLevel;
        }
        totalFear /= otherColonyInfos.Count;


        // expansion infos


         valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.offensive] = totalFear *0.5f;

        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive] = totalFear *0.40f;
      

    }
    
  public  float economicPressureCalc()
    {
        TriValueStruct incomeRatios = thisColonyScript.incomeToExpensesRatios();
        float totalRatio = (incomeRatios.moneyValue + incomeRatios.resourceValue + incomeRatios.populationValue) / 3f;

        TriValueStruct ticksTillBankruptcy = thisColonyScript.resourcesOwned.divide(thisColonyScript.totalIncome());
        TriValueStruct scarcity = TriValueStruct.one.multiply(30).divide(ticksTillBankruptcy);
         float totalScarcity = scarcity.Average();
    
        float pressure = 0.5f * (1 - totalRatio) + 0.5f * totalScarcity;
        


        return pressure;
      

    }
    void updateGoalPressure()
    {
      
       
        foreach(AgentGoal goal in goals)
        {
            Debug.Log("im about to eval");
            switch (goal.Name)
            {
                case "militaryPressure":
                
                    goal.priority = Mathf.Clamp01((otherColonyInfos.Sum(x=>x.threatLevel) * aggression)/ otherColonyInfos.Count);
                    
                    break;
                case "economicPressure":
                  float economicPressure = economicPressureCalc();
                    goal.priority = Mathf.Clamp01(economicPressure);
                    break;
                case "expansionPressure":
                    goal.priority = Mathf.Clamp01 (1 - thisColonyScript.totalIncome().populationValue / (thisColonyScript.allTilesOwned.Count * 10));
                    break;
            }
            Debug.Log($"goal is: {goal.Name} and the priotry is {goal.priority}");
        }
       
    }
    
  
    public bool hasntWaited = true;
    void setupBeliefs()
    
    {
        beliefs = new Dictionary<string, agentBelief>();
        beliefFactory factory = new beliefFactory(this, beliefs);

        factory.addBeliefs("Nothing", () => false);
        factory.addBeliefs("is feeling safe", () => false);// maybe make it an actual conditional at some point? 
        factory.addBeliefs("has good economy", () => false);
        factory.addBeliefs("satisfied with size", () => false);
 
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

   ////////////////////////////////////////////////////////////////////////// purpose specific beliefs
   bool hasPurposeNeeded(buildableScript.AIBuildableInfo.buildablePurposes purposeWanted)
    {
            foreach(buildableScript buildable in thisColonyScript.ownedBuildables.Select(x=>x.GetComponent<buildableScript>()))
            {
                if(buildable.isPerformingActions == true)
                {
                    continue;
                }
                return buildable.purposes.Select(x=> x.purpose).Contains(purposeWanted); 
            }
            return false;

    }
        
     factory.addBeliefs("has settlers", () => hasPurposeNeeded(buildableScript.AIBuildableInfo.buildablePurposes.expansion));

      factory.addBeliefs("has missiles", () => hasPurposeNeeded(buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive));

        
         
       




    }
    
    void setupActions()
    {
        actions = new HashSet<agentAction>();


        
        //globally used beliefs
        ///////////////////////////////////////////////////////////////////
        

        actions.Add(new agentAction.Builder("make space")
        .WithStrat(new makeSpaceStrat(thisColonyScript))
        .AddEffect(beliefs["has space to build"])
        .Build());
        
        actions.Add(new agentAction.Builder("do nothing")
        .WithStrat(new waitTickStrat(2))
        .AddEffect(beliefs["Nothing"])
        .Build());

        
 
        
    

        ////////////////////////////////////////////////////////////////////////
      

        //purpose specific actions
        ////////////////////////////////////////////////////////////////////////
        
        /// expansion
        actions.Add(new agentAction.Builder("build settlers")
        .WithStrat(new chooseAndBuildBuildableStrat(this,buildableScript.AIBuildableInfo.buildablePurposes.expansion))
        .addPreCondition(beliefs["has space to build"])
        .AddEffect(beliefs["has settlers"])
        .Build());


       actions.Add(new agentAction.Builder("settle new land")
        .WithStrat(new settlerUseStrat(this,buildableScript.AIBuildableInfo.buildablePurposes.expansion,buildableScript.buildableActions.GenericAction,5,thingsToTargetEnum.tiles))
        .AddEffect(beliefs["satisfied with size"])
        .addPreCondition(beliefs["has settlers"])
        .Build());


        /// Missiles
         actions.Add(new agentAction.Builder("build missiles")
        .WithStrat(new chooseAndBuildBuildableStrat(this,buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive))
        .addPreCondition(beliefs["has space to build"])
        .AddEffect(beliefs["has missiles"])
        .Build());

         actions.Add(new agentAction.Builder("bomb enemy to ashes")
        .WithStrat(new bombEnemiesStrat(this,buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive,buildableScript.buildableActions.launch,thingsToTargetEnum.both,0.45f))
        .AddEffect(beliefs["is feeling safe"])
        .addPreCondition(beliefs["has missiles"])
        .Build());

        ///economy
         actions.Add(new agentAction.Builder("build economy")
         .WithStrat(new chooseAndBuildBuildableStrat(this,buildableScript.AIBuildableInfo.buildablePurposes.economy))
         .AddEffect(beliefs["has good economy"])
         .Build());


        
        
        

       
        
        
        
        
        
        
       //.Build());


    }

   
    void setupGoals()
    {

        goals = new HashSet<AgentGoal>();
        

        // how to change goal pressure: you have to 
        
        goals.Add(new AgentGoal.Builder("militaryPressure")
        .withPriority(1f)
        .withdesiredEffects(beliefs["is feeling safe"])
        .Build());

        goals.Add(new AgentGoal.Builder("economicPressure")
        .withPriority(0.30f)
        .withdesiredEffects(beliefs["has good economy"])
        .Build());

        goals.Add(new AgentGoal.Builder("expansionPressure")
        .withPriority(0f)
        .withdesiredEffects(beliefs["satisfied with size"])
        .Build());

    }
    






    void colonyAiTick()
    {
        
        
        
            //statsTimer.tick(Time.deltaTime);
        if (currentAction == null)
        {
            Debug.Log("calculating any potential new plan");
            calculatePlan();
            if (actionplan != null && actionplan.Actions.Count > 0)
            {
                currentGoal = actionplan.AgentGoal;
               
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
                if (infoStuct.purpose == dog && infoStuct.strength > strengthRequired)
                {
                    selectedBuildables.Add(currentscript);
                   

                }

            }


        }
        return selectedBuildables.ToArray();
        

    }
    public buildableScript[] getTypeOfBuildable(buildableScript.AIBuildableInfo.buildablePurposes dog)
    {
    
        List<buildableScript> selectedBuildables = new List<buildableScript>();
        var dic = buildablesPurposesGrouped.buildablePurposeDictonary;
       List<buildableGameObject> listOfBuildablesObjects =  dic[dog];
    
       foreach(buildableGameObject current in listOfBuildablesObjects)
        {
            buildableScript dsafjdbsnfn = current.buildableObject.GetComponent<buildableScript>();
            selectedBuildables.Add(dsafjdbsnfn);
        }

       
        return selectedBuildables.ToArray();
    }
    public buildableGameObject[] getTypeOfBuildableObject(buildableScript.AIBuildableInfo.buildablePurposes dog)
    {
    
        List<buildableGameObject> selectedBuildables = new List<buildableGameObject>();
        var dic = buildablesPurposesGrouped.buildablePurposeDictonary;
       List<buildableGameObject> listOfBuildablesObjects =  dic[dog];
    
       foreach(buildableGameObject current in listOfBuildablesObjects)
        {
            
            selectedBuildables.Add(current);
        }

       
        return selectedBuildables.ToArray();
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





    



