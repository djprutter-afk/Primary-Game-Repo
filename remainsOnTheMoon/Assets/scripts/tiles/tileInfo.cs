using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class tileInfo : MonoBehaviour
{
   
    public bool occupid = false;
   
    public float populationGrowthPercent = 1.003f;
    public List<BuildingStruct> buildingsOnTile = new List<BuildingStruct>();


    public float resource;

    public float resourceModifyer = 1;// modifyed by bonuss
    public float development;
    public float population;
    public MoonScript theMoon;
    GameObject ownerColony;
    colonyScript ownerColonyScript;
   
    void Start()
    {
       
        
        ownerColony = transform.parent.gameObject;

        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
   



    }

    public BuildingStruct TotalIncome(bool alsoAdd = false)// this is the formula for determining all products of the tile
    {
        ownerColony = transform.parent.gameObject;// update owner cause might change 
        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
        float moneyGainDollars = (development * population) / 18 + 5;
        float resourceProduction = resource * resourceModifyer * (population / 8);

        float totalPopGrowth = 1;
        if (ownerColonyScript != null)
        {
            totalPopGrowth = population *ownerColonyScript.totalColonyPopGrowth *  ((1 -(population/(development*300 + 50)))/15);// redo sometime to be better
            

        }


        foreach (BuildingStruct building in buildingsOnTile)
        {
            moneyGainDollars -= building.moneyExpenses;
            resourceProduction -= building.resourceExpenses;
            totalPopGrowth -= building.populationExpenses;

        }

        if (alsoAdd == true)
        {
            
            population += totalPopGrowth;
            if(population <= 0)// a state cannot cannot express it's authority without people
            {
                deSettle();
            }
            ownerColonyScript.resourcesOwned.moneyExpenses += moneyGainDollars;
            ownerColonyScript.resourcesOwned.resourceExpenses += resourceProduction;



        }
        

        BuildingStruct total = new BuildingStruct
        {
            moneyExpenses = moneyGainDollars,
            resourceExpenses = resourceProduction,
            populationExpenses = totalPopGrowth

        };

        return total;






    }
    /// <summary>
    /// FINSIS THIS NOW DANIEL
    /// </summary>
    void deSettle()
    {
        tileVisuals TileVisual = gameObject.GetComponent<tileVisuals>();
        TileVisual.setupTileVisuals(theMoon.moonMaterial);
        transform.SetParent(theMoon.transform);
    }




/// <summary>
/// DOES NOT TAKE MON/RES/POP AWAY!!!
/// </summary>
/// <param name="thisBuildable"></param>
/// <param name="thisColony"></param>
/// <returns></returns>
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




[System.Serializable]
public struct BuildingStruct// poorly named, variables are also poorly named but i dont wanna change it
{
    public float moneyExpenses;
    public float resourceExpenses;
    public float populationExpenses; // people sacrfice for the gods
    public string buildingName;
    public BuildingStruct multiply(float multiplier, bool apply = false)
    {
         BuildingStruct buildingStruct = new BuildingStruct();
        buildingStruct.moneyExpenses = moneyExpenses * multiplier;
        buildingStruct.resourceExpenses = resourceExpenses * multiplier;
        buildingStruct.populationExpenses = populationExpenses * multiplier;
        if (apply == true)
        {
            moneyExpenses *= multiplier;
            resourceExpenses *= multiplier;
            populationExpenses *= multiplier;
        }
        return buildingStruct;
    }
    public BuildingStruct divide(BuildingStruct divisor, bool apply = false)
    {
         BuildingStruct buildingStruct = new BuildingStruct();
        buildingStruct.moneyExpenses = moneyExpenses / divisor.moneyExpenses;
        buildingStruct.resourceExpenses = resourceExpenses / divisor.resourceExpenses;
        buildingStruct.populationExpenses = populationExpenses / divisor.populationExpenses;
        if (apply == true)
        {
            moneyExpenses /= divisor.moneyExpenses;
            resourceExpenses /= divisor.resourceExpenses;
            populationExpenses /= divisor.populationExpenses;
        }
        return buildingStruct;
    }
    public BuildingStruct subtract(BuildingStruct subrtract, bool apply = false)
    {
        BuildingStruct buildingStruct = new BuildingStruct();
        buildingStruct.moneyExpenses = moneyExpenses - subrtract.moneyExpenses;
        buildingStruct.resourceExpenses = resourceExpenses - subrtract.resourceExpenses;
        buildingStruct.populationExpenses = populationExpenses - subrtract.populationExpenses;
        if (apply == true)
        {
            moneyExpenses -= subrtract.moneyExpenses;
            resourceExpenses -= subrtract.resourceExpenses;
            populationExpenses -= subrtract.populationExpenses;
        }
        return buildingStruct;
        
    }
    public BuildingStruct addition(BuildingStruct add, bool apply = false)
    {
        BuildingStruct buildingStruct = new BuildingStruct();
        buildingStruct.moneyExpenses = moneyExpenses + add.moneyExpenses;
        buildingStruct.resourceExpenses = resourceExpenses + add.resourceExpenses;
        buildingStruct.populationExpenses = populationExpenses + add.populationExpenses;
        if (apply == true)
        {
            moneyExpenses += add.moneyExpenses;
            resourceExpenses += add.resourceExpenses;
            populationExpenses += add.populationExpenses;
        }
        return buildingStruct;
        
    }
    /// <summary>
    /// if firstcost is greater in every way than secondcost then true, elsewise false :()
    /// </summary>
    /// <param name="firstCost"></param>
    /// <param name="secondCost"></param>
    /// <returns></returns>
    public static bool comapareCosts(BuildingStruct firstCost,BuildingStruct secondCost = new BuildingStruct(),bool alsoSubtract = false)
    {
       
     

        if (firstCost.moneyExpenses < secondCost.moneyExpenses)
        {
            Debug.LogError("FAILED MONEYWiSe");
            return false;
        }
         if (firstCost.resourceExpenses < secondCost.resourceExpenses) 
        {Debug.LogError("FAILED RESOURCEWISe");
            return false;
        }
        if (firstCost.populationExpenses < secondCost.populationExpenses)
        {Debug.LogError("FAILED POPWISE");
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
