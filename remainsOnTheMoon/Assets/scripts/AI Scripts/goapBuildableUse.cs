using UnityEngine;

using System.Collections.Generic;

using System.Linq;


public abstract class buildableUseStrat : iActionStrat
{
    bool finishedAction = false;
    public bool canPerform => !complete;
    public bool complete => finishedAction;
    public baseColonyAI colonyAI;
buildableScript.AIBuildableInfo.buildablePurposes buildablePurposeNeeded;
buildableScript.buildableActions actionToUse;



    public  buildableUseStrat(baseColonyAI ColonyAI, buildableScript.AIBuildableInfo.buildablePurposes BuildablePurposeNeeded,buildableScript.buildableActions actionToUse)
    {
        colonyAI = ColonyAI;
        buildablePurposeNeeded = BuildablePurposeNeeded;
        this.actionToUse = actionToUse;
        
      
    }
    public virtual UnityEngine.Vector3[] postionDecider()
    {
        UnityEngine.Vector3[] position = new UnityEngine.Vector3[1];

        position[0] = UnityEngine.Vector3.zero;
        return position;
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
       
        UnityEngine.Vector3[] positionsToUse = postionDecider();
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

public class settlerUseStrat : buildableUseStrat
{

    int positionsToFind;
        public settlerUseStrat(baseColonyAI colonyAI, buildableScript.AIBuildableInfo.buildablePurposes purpose,buildableScript.buildableActions actionToUse,int PositionsToFind)
        : base(colonyAI, purpose, actionToUse)
    {
        positionsToFind = PositionsToFind;
       
    }
    public override Vector3[] postionDecider()
    {
       
         GameObject[] sortedGameObjects = new GameObject[positionsToFind];// end result

        colonyScript colonyScript = colonyAI.gameObject.GetComponent<colonyScript>();
        GameObject[] outlineTiles = colonyMethoods.findOUterEdgeTiles(colonyAI.gameObject);
        int numberOfOutlineTiles = outlineTiles.Length;

        if(positionsToFind > outlineTiles.Length)
        {
            positionsToFind =  outlineTiles.Length;
        }


        if (numberOfOutlineTiles <= 0)
        {
            Debug.LogError("not enough tiless to evalute");
            return null;
        }
        Dictionary<GameObject, float> dictonaryOfTiles = new Dictionary<GameObject, float>();



        for (int indexOfOutlineTiles = 0; indexOfOutlineTiles < numberOfOutlineTiles; indexOfOutlineTiles++)
        {
            float tileValue = 0;
            GameObject currentOutLineTile = outlineTiles[indexOfOutlineTiles];

            tileInfo currentTileInfo = currentOutLineTile.GetComponent<tileInfo>();

            tileValue += currentTileInfo.resource;

            Collider[] tilesSurroundingCurrent = Physics.OverlapSphere(currentOutLineTile.transform.position, 0.05f);


            for (int k = 0; k < tilesSurroundingCurrent.Length; k++)
            {


                if (tilesSurroundingCurrent[k].transform.parent.gameObject == colonyAI.transform.gameObject)
                {

                    tileValue += (1 - colonyAI.aggression) * 2;

                }
            }
          
            if (dictonaryOfTiles.ContainsKey(currentOutLineTile) == false && colonyScript.allTilesOwned.Contains(currentOutLineTile) == false)
            {
                dictonaryOfTiles.Add(currentOutLineTile, tileValue);

            }
        }

            List<KeyValuePair<GameObject, float>> kvpOfTiles = dictonaryOfTiles.ToList();

            var sortedKvpOfTiles = kvpOfTiles.OrderByDescending(pair => pair.Value).ToList();

           

            for(int i = 0; i < positionsToFind ;i++)
            {
                sortedGameObjects[i] = sortedKvpOfTiles[i].Key;
            }

          

        
         return sortedGameObjects.Select(x=>x.transform.position).ToArray();


    }
 


    public static GameObject[] allColonyTiles(GameObject colony)
    {

        int amtOfChildrend = colony.transform.childCount;
        GameObject[] arrayTempTiles = new GameObject[amtOfChildrend];
        if (amtOfChildrend <= 0)
        {
            Debug.LogError("colony: " + colony + " attempted to get the size of their empire even though they have none");
            return null;
        }
        for (int p = 0; p < amtOfChildrend; p++)
        {
            arrayTempTiles[p] = colony.transform.GetChild(p).gameObject;


        }
        return arrayTempTiles;
    }


    public static bool purchasableAction(GameObject colony, TriValueStruct cost, GameObject tileOn, bool alsoBuy = false)
    {
        tileInfo tileOnInfo = tileOn.GetComponent<tileInfo>();
        Debug.Log(cost.moneyValue + " " + colony.gameObject.name + " " + tileOn.transform.parent);
        colonyScript thiscolonyScript = colony.GetComponent<colonyScript>();

        if (colony != tileOn.transform.parent.gameObject)
        {
            return false;
        }

        if (thiscolonyScript == null)
        {

            Debug.LogWarning("there was no colony script on the colony????");
            return false;
        }
        bool enoughResources = thiscolonyScript.resourcesOwned.resourceValue >= cost.resourceValue;
        bool enoughMOney = thiscolonyScript.resourcesOwned.moneyValue >= cost.moneyValue;
        bool enoughPeople = tileOnInfo.population >= cost.populationValue;

        bool allGood = enoughResources && enoughMOney && enoughPeople;



        if (allGood)
        {
            if (alsoBuy == false)
            {
                return true;
            }


            thiscolonyScript.resourcesOwned.resourceValue -= cost.resourceValue;
            thiscolonyScript.resourcesOwned.moneyValue -= cost.moneyValue;
            tileOnInfo.population -= cost.populationValue;
            return true;

        }
        else
        {
            return false;
            //BROKIE
        }
    }
}
