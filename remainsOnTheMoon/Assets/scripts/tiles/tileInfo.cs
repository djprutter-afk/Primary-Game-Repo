using System.Collections.Generic;
using UnityEngine;


public class tileInfo : MonoBehaviour
{
    public GameObject visualMoon;
   
    public bool occupid = false;
    public float tileProtection = 0f;
    public float populationGrowthPercent = 1.003f;
    public List<BuildingStruct> buildingsOnTile = new List<BuildingStruct>();


    public float resource;

    public float resourceModifyer = 1;// modifyed by bonuss
    public float development;
    public int population;
    GameObject ownerColony;
    colonyScript ownerColonyScript;
    public GameObject visualTile;
    void Start()
    {
       
        
        ownerColony = transform.parent.gameObject;

        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
        buildingSuperStruct development = new buildingSuperStruct
        {
            upkeep = new BuildingStruct
            {
                buildingName = "local admin building",
                moneyExpenses = 20,
                resourceExpenses = 20,
                populationExpenses = 1
            }


        };




    }

    public BuildingStruct TotalIncome(bool alsoAdd = false)// this is the formula for determining all products of the tile
    {
        ownerColony = transform.parent.gameObject;// update owner cause might change 
        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
        float moneyGainDollars = (development * population) / 33 + 5;
        float resourceProduction = resource * resourceModifyer * (population / 15);

        int totalPopGrowth = 1;
        if (ownerColonyScript != null)
        {
            totalPopGrowth = (int)(population * populationGrowthPercent * ownerColonyScript.totalColonyPopGrowth);// redo sometime to be better

        }


        foreach (BuildingStruct building in buildingsOnTile)
        {
            moneyGainDollars -= building.moneyExpenses;
            resourceProduction -= building.resourceExpenses;
            totalPopGrowth -= building.populationExpenses;

        }

        if (alsoAdd == true)
        {
            population = totalPopGrowth;
            ownerColonyScript.resourcesOwned.moneyExpenses += moneyGainDollars;
            ownerColonyScript.resourcesOwned.resourceExpenses += resourceProduction;



        }

        BuildingStruct total = new BuildingStruct
        {
            moneyExpenses = moneyGainDollars,
            resourceExpenses = resourceProduction,
            populationExpenses = population

        };

        return total;






    }





    public GameObject buildNewBuildable(buildableGameObject thisBuildable, colonyScript thisColony)
    {
        GameObject newBuildable = Instantiate(thisBuildable.buildableObject, thisColony.gameObject.transform);

        Debug.Log(newBuildable);

        buildableScript newBuildableScript = newBuildable.GetComponent<buildableScript>();

        //newBuildableScript.isBuilding = thisBuildable.isBuilding;

        thisColony.ownedBuildables.Add(newBuildable);

  

        newBuildableScript.tileOn = gameObject;

        newBuildableScript.becomeParellel();
        if (newBuildableScript.isBuilding == true)
        {

            buildingsOnTile.Add(newBuildableScript.upkeepCosts);
            

        }
        return newBuildable;
                

    }
    


}


struct buildingSuperStruct
{
    public BuildingStruct upkeep;
    public GameObject buildingObject;
}



[System.Serializable]
public struct BuildingStruct// poorly named, variables are also poorly named but i dont wanna change it
{
    public float moneyExpenses;
    public float resourceExpenses;
    public int populationExpenses; // people sacrfice for the gods
    public string buildingName;
    /// <summary>
    /// if firstcost is greater in every way than secondcost then true, elsewise false :()
    /// </summary>
    /// <param name="firstCost"></param>
    /// <param name="secondCost"></param>
    /// <returns></returns>
    public static bool comapareCosts(BuildingStruct firstCost,BuildingStruct secondCost,bool alsoSubtract = false)
    {
     

        if (firstCost.moneyExpenses < secondCost.moneyExpenses)
        {
            return false;
        }
         if (firstCost.resourceExpenses < secondCost.resourceExpenses) 
        {
            return false;
        }
        if (firstCost.populationExpenses < secondCost.populationExpenses)
        {
            return false;
        }
        if(alsoSubtract)
        {
            firstCost.moneyExpenses -= secondCost.moneyExpenses;
            firstCost.resourceExpenses -= secondCost.resourceExpenses;
            firstCost.populationExpenses -= secondCost.populationExpenses;
            
        }
       
        return true;

        
    }

}
