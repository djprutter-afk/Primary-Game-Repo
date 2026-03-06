using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;


public class tileInfo : MonoBehaviour
{
   
    public bool occupid = false;
   
    public float populationGrowthPercent = 1.003f;
    public List<TriValueStruct> buildingsOnTile = new List<TriValueStruct>();


    public float resource;

    public float resourceModifyer = 1;// modifyed by bonuss
    public float development;
    public float population;
    public MoonScript theMoon;
    GameObject ownerColony;
    colonyScript ownerColonyScript;
    
    private float damageRecoveryTimer = 0f;
    private float damageRecoveryDuration = 30f; // Seconds to recover
    private float populationGrowthMultiplier = 0f; // Added/subtracted from growth during recovery
   
    void Start()
    {
       
        
        ownerColony = transform.parent.gameObject;

        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
   



    }

    public TriValueStruct TotalIncome(bool alsoAdd = false)// this is the formula for determining all products of the tile
    {
        ownerColony = transform.parent.gameObject;// update owner cause might change 
        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
        float moneyGainDollars = (development * population) / 18 + 5;
        float resourceProduction = resource * resourceModifyer * (population / 8);

        float totalPopGrowth = 1;
        if (ownerColonyScript != null)
        {
            // Reduce growth significantly at lower populations using square root scaling
            float populationScaling = Mathf.Sqrt(population / 10f); // Heavily penalizes low population
            // Development acts as hard cap on population growth
            float developmentCap = Mathf.Max(0, 1f - (population / (development * 50f))); // Much stricter development constraint
            totalPopGrowth = population *ownerColonyScript.totalColonyPopGrowth * developmentCap * populationScaling + populationGrowthMultiplier;// redo sometime to be better
            
            // Recover from damage over time
            if (damageRecoveryTimer > 0)
            {
                damageRecoveryTimer -= Time.deltaTime;
                // Simple linear recovery: penalty decreases each frame
                populationGrowthMultiplier = Mathf.Lerp(0f, populationGrowthMultiplier, damageRecoveryTimer / damageRecoveryDuration);
            }
            else
            {
                populationGrowthMultiplier = 0f;
            }

        }


        foreach (TriValueStruct building in buildingsOnTile)
        {
            moneyGainDollars -= building.moneyValue;
            resourceProduction -= building.resourceValue;
            totalPopGrowth -= building.populationValue;

        }

        if (alsoAdd == true)
        {
            
            population += totalPopGrowth;
            if(population < 1 || population == null)// a state cannot cannot express it's authority without people
            {
                ownerColonyScript.allTilesOwned.Remove(gameObject);
                deSettle();
                return new TriValueStruct();
            }
            ownerColonyScript.resourcesOwned.moneyValue += moneyGainDollars;
            ownerColonyScript.resourcesOwned.resourceValue += resourceProduction;



        }
        

        TriValueStruct total = new TriValueStruct
        {
            moneyValue = moneyGainDollars,
            resourceValue = resourceProduction,
            populationValue = totalPopGrowth

        };

        return total;






    }
 
    void deSettle()
    {
        population = 0;
        tileVisuals TileVisual = gameObject.GetComponent<tileVisuals>();
        TileVisual.setupTileVisuals(theMoon.moonMaterial);
        transform.SetParent(theMoon.transform);
    }
/// <summary>
/// Damages the tile based on explosion power, reducing population, development, and destroying buildings, this function was entirely made by ai, im sorry i was tired
/// </summary>
/// <param name="power">should range with explosion power</param>
    public void damageTile(float power)
    {
        // Reduce population based on explosion power (casualties) - MORE EXTREME
        float populationLoss = population * (power / 30f); // Increased damage from 100 to 30
        population = Mathf.Max(0, population - populationLoss);
        
        // Reduce development (infrastructure damage)
        float developmentLoss = development * (power / 150f);
        development = Mathf.Max(0, development - developmentLoss);
        
        // Reduce resources (resource destruction)
        float resourceLoss = resource * (power / 200f);
        resource = Mathf.Max(0, resource - resourceLoss);
        
        // Damage/destroy buildings on the tile
        for (int i = buildingsOnTile.Count - 1; i >= 0; i--)
        {
            if (UnityEngine.Random.value < Mathf.Min(power / 100f, 1f)) // Higher power = higher chance to destroy building
            {
                buildingsOnTile.RemoveAt(i);
            }
        }
        
        // Temporarily reduce population growth during recovery
        damageRecoveryTimer = damageRecoveryDuration;
        // Negative growth penalty: higher development = faster recovery
        // Development acts as cushion against negative growth
        float developmentFactor = Mathf.Clamp01(development / 100f); // 0-1 scale
        populationGrowthMultiplier = -(power / 20f) * (1f - developmentFactor * 0.7f); // Dev reduces penalty by up to 70%
        
        // If all population is dead, desettle the tile
        if (population <= 0)
        {
            deSettle();
        }
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
public struct TriValueStruct
{
    public float moneyValue;
    public float resourceValue;
    public float populationValue; 
    public string buildingName;
    public TriValueStruct multiply(float multiplier, bool apply = false)
    {
         TriValueStruct buildingStruct = new TriValueStruct();
        buildingStruct.moneyValue = moneyValue * multiplier;
        buildingStruct.resourceValue = resourceValue * multiplier;
        buildingStruct.populationValue = populationValue * multiplier;
        if (apply == true)
        {
            moneyValue *= multiplier;
            resourceValue *= multiplier;
            populationValue *= multiplier;
        }
        return buildingStruct;
    }
    public TriValueStruct divide(TriValueStruct divisor, bool apply = false)
    {
         TriValueStruct buildingStruct = new TriValueStruct();
        buildingStruct.moneyValue = moneyValue / divisor.moneyValue;
        buildingStruct.resourceValue = resourceValue / divisor.resourceValue;
        buildingStruct.populationValue = populationValue / divisor.populationValue;
        if (apply == true)
        {
            moneyValue /= divisor.moneyValue;
            resourceValue /= divisor.resourceValue;
            populationValue /= divisor.populationValue;
        }
        return buildingStruct;
    }
    public TriValueStruct subtract(TriValueStruct subrtract, bool apply = false)
    {
        TriValueStruct buildingStruct = new TriValueStruct();
        buildingStruct.moneyValue = moneyValue - subrtract.moneyValue;
        buildingStruct.resourceValue = resourceValue - subrtract.resourceValue;
        buildingStruct.populationValue = populationValue - subrtract.populationValue;
        if (apply == true)
        {
            moneyValue -= subrtract.moneyValue;
            resourceValue -= subrtract.resourceValue;
            populationValue -= subrtract.populationValue;
        }
        return buildingStruct;
        
    }
    public TriValueStruct addition(TriValueStruct add, bool apply = false)
    {
        TriValueStruct buildingStruct = new TriValueStruct();
        buildingStruct.moneyValue = moneyValue + add.moneyValue;
        buildingStruct.resourceValue = resourceValue + add.resourceValue;
        buildingStruct.populationValue = populationValue + add.populationValue;
        if (apply == true)
        {
            moneyValue += add.moneyValue;
            resourceValue += add.resourceValue;
            populationValue += add.populationValue;
        }
        return buildingStruct;
        
    }
    public TriValueStruct normalize()
    {
        float[] valuesOfStruct = new float[3]{moneyValue,resourceValue,populationValue};
        
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        foreach(float value in valuesOfStruct)
        {
            if(value > maxValue)
            {
                maxValue = value;
                
            }
            if(value < minValue)
            {
                minValue = value;
                
            }
            
            
        }

        if(minValue == maxValue)
        {
            return this;
        }
         
        TriValueStruct normalizedTriValue = new TriValueStruct();
        
        float normalValue (float value) 
        {
           return (value - minValue)/(maxValue - minValue);

        }
        normalizedTriValue.moneyValue =normalValue(moneyValue);
        normalizedTriValue.resourceValue = normalValue(resourceValue);
        normalizedTriValue.populationValue = normalValue(populationValue);
            
        
        
     
        return normalizedTriValue;
        
    }
    public static TriValueStruct one = new TriValueStruct
    {
        moneyValue = 1,
        resourceValue = 1,
        populationValue = 1
        
    };
     public static TriValueStruct zero = new TriValueStruct
    {
        moneyValue = 0,
        resourceValue = 0,
        populationValue = 0
        
    };
    
    
    /// <summary>
    /// if firstcost is greater in every way than secondcost then true, elsewise false :()
    /// </summary>
    /// <param name="firstCost"></param>
    /// <param name="secondCost"></param>
    /// <returns></returns>
    public static bool comapareCosts(TriValueStruct firstCost,TriValueStruct secondCost = new TriValueStruct(),bool alsoSubtract = false)
    {
       
     

        if (firstCost.moneyValue < secondCost.moneyValue)
        {
            Debug.LogError("FAILED MONEYWiSe");
            return false;
        }
         if (firstCost.resourceValue < secondCost.resourceValue) 
        {Debug.LogError("FAILED RESOURCEWISe");
            return false;
        }
        if (firstCost.populationValue < secondCost.populationValue)
        {Debug.LogError("FAILED POPWISE");
            return false;
        }
        if(alsoSubtract)
        {
            firstCost.moneyValue -= secondCost.moneyValue;
            firstCost.resourceValue -= secondCost.resourceValue;
            firstCost.populationValue -= secondCost.populationValue;
            
        }
      
       
        return true;

        
    }

}
