using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;


public class tileInfo : MonoBehaviour
{
    public int maxPopAddedPerDevelopment = 250;
   
    public bool occupid = false;
   
    [SerializeField] float resourceExtractionModifier = 0.03f;
   
    public float ResourceRegenerationRate = 0.02f;
    float totalRegen;
    public float ResourceCapacity;
    public List<TriValueStruct> buildingsOnTile = new List<TriValueStruct>();


    public float development;
    public float population;
    public MoonScript theMoon;
    GameObject ownerColony;
    colonyScript ownerColonyScript;


    public float lastExtractionAmount {private set; get; }
    public float lastResourceRegeneration {private set; get; }
    
  
   
    void Start()
    {
       
        
        ownerColony = transform.parent.gameObject;

        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
   



    }

    public TriValueStruct TotalIncome(bool alsoAdd = false)// this is the formula for determining all products of the tile
    {
        Debug.Log("calculating income for tile " + gameObject.name);
        ownerColony = transform.parent.gameObject;// update owner cause might change 
        ownerColonyScript = ownerColony.GetComponent<colonyScript>();
        
       

        float totalPopGrowth = 1;
        Debug.Log("Initial totalPopGrowth: " + totalPopGrowth + ", development: " + development + ", population: " + population + ", maxPopAddedPerDevelopment: " + maxPopAddedPerDevelopment);
        if (ownerColonyScript != null)
        {
            // Carrying capacity based on development
            float carryingCapacity = development * maxPopAddedPerDevelopment;
            Debug.Log("carryingCapacity: " + carryingCapacity);
            float growthRate = 0.02f; 
            float growthModifier = 1f;
            if (carryingCapacity > 0)
            {
                growthModifier = Mathf.Max(0f, 1f - (population / carryingCapacity));
               
            }

            totalPopGrowth = population * growthRate * growthModifier;
           
        }
        else
        {
            Debug.LogError("owner colony script is null for tile " + gameObject.name);
        }
        float moneyGainDollars = 0;

        float TotalResourceExtraction = 0;
       
        foreach (TriValueStruct building in buildingsOnTile)
        {
           
            moneyGainDollars -= building.moneyValue;
            TotalResourceExtraction -= building.resourceValue;
            totalPopGrowth -= building.populationValue;
        }

        TotalResourceExtraction += resourceExtractionModifier*(population / Mathf.Clamp(development,1,float.MaxValue) * 0.5f); 

        moneyGainDollars += Mathf.Sqrt(population * development);
        totalRegen = ResourceRegenerationRate *ResourceCapacity;
        TotalResourceExtraction = Mathf.Min(TotalResourceExtraction, totalRegen);
        

        if (alsoAdd == true)
        {
            Debug.Log("alsoAdd is true, updating values");

            lastExtractionAmount = TotalResourceExtraction;
            lastResourceRegeneration = totalRegen;
           
            population += totalPopGrowth;
           
            if(population < 1)// a state cannot cannot express it's authority without people
            {
                Debug.Log("Population < 1, desettling tile");
                ownerColonyScript.allTilesOwned.Remove(gameObject);
                deSettle();
                return new TriValueStruct();
            }
            ownerColonyScript.resourcesOwned.moneyValue += moneyGainDollars;

            ownerColonyScript.resourcesOwned.resourceValue += TotalResourceExtraction;
           
        }
        else
        {
            Debug.Log("alsoAdd is false, not updating values");
        }
        
        TriValueStruct total = new TriValueStruct
        {
            moneyValue = moneyGainDollars,
            resourceValue = TotalResourceExtraction,
            populationValue = totalPopGrowth
        };
        Debug.Log("Total income: moneyValue: " + total.moneyValue + ", resourceValue: " + total.resourceValue + ", populationValue: " + total.populationValue);

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
/// Damages the tile based on explosion power, reducing population, development, and destroying buildings, this function was entirely made by ai
/// </summary>
/// <param name="power">should range with explosion power</param>
    public void damageTile(float power)
    {
        // Reduce population based on explosion power (casualties) - MORE EXTREME
        float populationLoss = population * (1- power ); // Increased damage from 100 to 30
        population = Mathf.Max(0, population - populationLoss);
        
        // Reduce development (infrastructure damage)
        float developmentLoss = development * (power / 0.5f);
        development = Mathf.Max(0, development - developmentLoss);
    
        
  
        float developmentFactor = Mathf.Clamp(development - power,0,float.MaxValue); // 0-1 scale
       
        
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
public struct TriValueStruct : IEnumerable<float>
{
    public float moneyValue;
    public float resourceValue;
    public float populationValue; 
    public string buildingName;

       public IEnumerator<float> GetEnumerator()
    {
        yield return moneyValue;
        yield return resourceValue; 
        yield return populationValue;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

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
     public TriValueStruct absolute( bool apply = false)
    {
        TriValueStruct buildingStruct = new TriValueStruct();
        buildingStruct.moneyValue = Mathf.Abs(moneyValue);
        buildingStruct.resourceValue = Mathf.Abs(resourceValue);
        buildingStruct.populationValue = Mathf.Abs(populationValue);
       
        if (apply == true)
        {
            moneyValue = buildingStruct.moneyValue;
            resourceValue = buildingStruct.resourceValue;
            populationValue =  buildingStruct.populationValue;
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
     public static  TriValueStruct one = new TriValueStruct
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
