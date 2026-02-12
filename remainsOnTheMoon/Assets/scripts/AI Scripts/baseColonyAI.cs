using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
/// <summary>
/// putting this here so i dont forget, money you should be able to go into debt but you cant go into debt for resource or population
/// </summary>
public class baseColonyAI : MonoBehaviour// high level decision maker for colony, does not directly control buildable but instead guides them
{
    int ticksToWait =2;

    int desiredSize;
    TriValueStruct desiredIncome;

    public buildableGameObject desiredBuildable = new buildableGameObject();
    public bool hasFreshDesiredbuildabe = false;

    

    
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
    TriValueStruct emptyStruct = new TriValueStruct();

         
    struct otherColonyInfo
    {
        public float threatLevel;
        public colonyScript colony {private get; set;}

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

List<otherColonyInfo> otherColonyInfos = new List<otherColonyInfo>();
    


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
   
    

        desiredIncome = new TriValueStruct
        {
            moneyValue = 30,
            resourceValue = 20,
            populationValue = 5
        };
        
        gameManagerScript.GameTick += colonyAiTick;
        
    }
    void setupJudgementSystem()
    {
        colonyScript[] allColonies = transform.parent.GetComponent<gameSetup1>().allColonieScripts.ToArray();
        foreach(colonyScript otherColony in allColonies)
        {
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
        statsTimer = new CountDownTimer(2f);
        statsTimer.onTimerEnd += () =>
        {
            updateValues();
            statsTimer.Start();
        };
        statsTimer.Start();

    }
    
  
    void updateValues()
    {
        desiredIncome.multiply(1.003f);
        TriValueStruct satifcation = thisColonyScript.totalIncome().divide(desiredIncome);
        float totalSatisfaction = (satifcation.moneyValue + satifcation.resourceValue + satifcation.populationValue) / 3f;

        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.economy] += 0.05f * (1 - totalSatisfaction *2);
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

        valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.offensive] = totalFear *0.5f;
      valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.expansion]  = 3- thisColonyScript.allTilesOwned.Count/5f;
 
    }
    
    
  
    public bool hasntWaited = true;
    void setupBeliefs()
    {
        beliefs = new Dictionary<string, agentBelief>();
        beliefFactory factory = new beliefFactory(this, beliefs);

        factory.addBeliefs("Nothing", () => false);

       
        factory.addBeliefs("is feeling secure", () => 
        TriValueStruct.comapareCosts(thisColonyScript.resourcesOwned.addition(thisColonyScript.totalIncome().multiply(15)),emptyStruct));

        factory.addBeliefs("is feeling insecure", () => 
        TriValueStruct.comapareCosts(thisColonyScript.resourcesOwned.addition(thisColonyScript.totalIncome().multiply(15)),emptyStruct) == false);

       factory.addBeliefs("has good economy", () => TriValueStruct.comapareCosts(thisColonyScript.resourcesOwned,desiredIncome));
       factory.addBeliefs("can afford ecoBuildable", () => TriValueStruct.comapareCosts(thisColonyScript.resourcesOwned,desiredBuildable.buildCost));
       factory.addBeliefs("cant afford ecoBuildable", () => TriValueStruct.comapareCosts(thisColonyScript.resourcesOwned,desiredBuildable.buildCost) == false);

       
   
       factory.addBeliefs("has Settlers", () => getTypeOfBuildableOwned(buildableScript.AIBuildableInfo.buildablePurposes.expansion).Length > 0);
        
        factory.addBeliefs("satisfied with buildables", () => false); // ai can never be satiated
        factory.addBeliefs("satisfied with size", () => false);// ai can never be satiated
        
      //  factory.addBeliefs("is feeling miltarily safe", () => isFeelingMiltarilySafe());

       // bool isFeelingMiltarilySafe()
        //{
            
        //}

        factory.addBeliefs("hasnt waited already", () => hasntWaited);// ai can never be satiated
        
        

        factory.addBeliefs("can afford new tile", () => TriValueStruct.comapareCosts(thisColonyScript.totalIncome(),emptyStruct));
         factory.addBeliefs("has decided on buildable", () => hasFreshDesiredbuildabe);
        factory.addBeliefs("has space to build", hasSpaceToBuild);
        factory.addBeliefs("has decided on buildable ECO", ()=>hasFreshDesiredbuildabe);

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
        .WithStrat(new makeSpaceStrat(thisColonyScript))
        .AddEffect(beliefs["has space to build"])
        .Build());
        
        actions.Add(new agentAction.Builder("do nothing")
        .WithStrat(new waitTickStrat(2))
        .AddEffect(beliefs["Nothing"])
        .Build());

        actions.Add(new agentAction.Builder("decide Buildable")
        .WithStrat(new chooseBuildableStrat(this))
        .addPreCondition(beliefs["is feeling secure"])
        .AddEffect(beliefs["has decided on buildable"])
        .Build());
       
        actions.Add(new agentAction.Builder("do nothing")
        .WithStrat(new waitTickStrat(2))
        .AddEffect(beliefs["Nothing"])
        .Build());
       

        actions.Add(new agentAction.Builder("buildBuildable")
        .WithStrat(new buildStrat(gameObject,this))
        .addPreCondition(beliefs["has decided on buildable"])
        .addPreCondition(beliefs["has space to build"])
        .AddEffect(beliefs["satisfied with buildables"])
        .Build());
        
        actions.Add(new agentAction.Builder("decide Buildable ECO")
        .WithStrat(new chooseBuildableecoStrat(this))
        .addPreCondition(beliefs["is feeling insecure"])
        .AddEffect(beliefs["has decided on buildable ECO"])
        .Build());

        actions.Add(new agentAction.Builder("buildBuildableECO")
        .WithStrat(new buildStrat(gameObject,this))
        .addPreCondition(beliefs["has decided on buildable ECO"])
        .addPreCondition(beliefs["has space to build"])
        .addPreCondition(beliefs["can afford ecoBuildable"])
        .AddEffect(beliefs["has good economy"])
        .Build());
         actions.Add(new agentAction.Builder("wait to afford eco buildable")
        .WithStrat(new decideTimeTowait(this))
        .addPreCondition(beliefs["hasnt waited already"])
        .addPreCondition(beliefs["has decided on buildable ECO"])
        .addPreCondition(beliefs["cant afford ecoBuildable"])
        .AddEffect(beliefs["can afford ecoBuildable"])
        .Build());

       actions.Add(new agentAction.Builder("settle new land")
        .WithStrat(new massUseStrat(this,buildableScript.AIBuildableInfo.buildablePurposes.expansion,buildableScript.buildableActions.GenericAction))
        .AddEffect(beliefs["satisfied with size"])
        .addPreCondition(beliefs["has Settlers"])
        .Build());
        
        
        
        
        
        
       //.Build());


    }

    void setupGoals()
    {

        goals = new HashSet<AgentGoal>();
        

        goals.Add(new AgentGoal.Builder("do nothing")
        .withPriority(0.1f)
        .withdesiredEffects(beliefs["Nothing"])
        .Build());
        
        goals.Add(new AgentGoal.Builder("make more buildables")
        .withPriority(0.25f)
        .withdesiredEffects(beliefs["satisfied with buildables"])
        .Build());

        goals.Add(new AgentGoal.Builder("improve economy")
        .withPriority(0.30f)
        .withdesiredEffects(beliefs["has good economy"])
        .Build());

        
       


        goals.Add(new AgentGoal.Builder("settle land")
        .withPriority(1.0f)
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





    



