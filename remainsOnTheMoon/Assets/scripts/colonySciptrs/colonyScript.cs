using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using UnityEngine.Tilemaps;



/// <summary>
/// very important script, this script is the script will contain everything for needed for each colony. for players and ai alike
/// </summary>





public class colonyScript : MonoBehaviour
{
    public event Action colonyResorceUpdate;
    /// <summary>
    /// first string is name, second is message
    /// </summary>
    public static event Action<string,string> newChatMsg;
    public List<GameObject> ownedEdgeTiles = new List<GameObject>();// tiles controlled other colonies

    public List<GameObject> outerUnownedTiles = new List<GameObject>(); // the surrounding tiles around the colony
    public List<GameObject> allTilesOwned = new List<GameObject>();
    public List<GameObject> ownedBuildables = new List<GameObject>();
    public Texture2D colonyFlag;

    public BuildingStruct resourcesOwned;
    public Material globalColonyMaterial;
    public Material LOCALColonyMaterial;
 

    
       colonyStart thisColonyStart;
    int tileAmount;
    public GameObject colonyGameManager;
    prices currentPrices;

    public float totalColonyPopGrowth = 1;
    public int tempGoapTestNumber = 0;




    void Start()
    {
        LOCALColonyMaterial = Instantiate(globalColonyMaterial);
        LOCALColonyMaterial.SetTexture("_flag", colonyFlag);
        currentPrices = colonyGameManager.GetComponent<prices>();
        thisColonyStart = GetComponent<colonyStart>();
        tileAmount = transform.childCount;
        resourcesOwned.resourceExpenses = 200;
        resourcesOwned.moneyExpenses = 450;
        allTilesOwned.Add(thisColonyStart.colonyStartPosition);


    }
    void OnDestroy()
    {
        Destroy(LOCALColonyMaterial);
    }

    public void resourceUpdate()// should not be used outside of parent's object script cause they control time or something
    {
        colonyResorceUpdate?.Invoke();
        tileAmount = allTilesOwned.Count;
        float totalPOP = 0;
        for (int indexOfTiles = 0; indexOfTiles < tileAmount; indexOfTiles++)
        {


            tileInfo thisTileInfo = allTilesOwned[indexOfTiles].GetComponent<tileInfo>();
            totalPOP += thisTileInfo.population;

            BuildingStruct tileincome = thisTileInfo.TotalIncome(true);

            float tileHousingPercentage = thisTileInfo.population / (thisTileInfo.development * 250 + 1);


            if (tileHousingPercentage > 1)
            {
                thisTileInfo.populationGrowthPercent -= 0.01f;//every update this decreases by 1 percent untill population is within margins
            }
            else if (tileHousingPercentage <= 1)
            {
                thisTileInfo.populationGrowthPercent = 1.01f;

            }



        }
        resourcesOwned.populationExpenses = totalPOP;
    }
    public BuildingStruct totalIncome()
    {
        BuildingStruct totalIncome = new BuildingStruct();
        foreach (GameObject buildable in ownedBuildables)
        {
            buildableScript thisBuildableScript = buildable.GetComponent<buildableScript>();
            if (thisBuildableScript.isBuilding)// if its building then it's already included in tiles
            {
                continue;
            }
            totalIncome.moneyExpenses -= thisBuildableScript.upkeepCosts.moneyExpenses;
            totalIncome.resourceExpenses -= thisBuildableScript.upkeepCosts.resourceExpenses;
            totalIncome.populationExpenses -= thisBuildableScript.upkeepCosts.populationExpenses;

        }
        foreach (GameObject tile in allTilesOwned)
        {
            tileInfo thisTileInfo = tile.GetComponent<tileInfo>();
            
            BuildingStruct totalTileIncome = thisTileInfo.TotalIncome();
            totalIncome.moneyExpenses += totalTileIncome.moneyExpenses;
            totalIncome.resourceExpenses += totalTileIncome.resourceExpenses;
            totalIncome.populationExpenses += totalTileIncome.populationExpenses;

        }
        Debug.LogWarning("total income is "+totalIncome.moneyExpenses + " " + totalIncome.resourceExpenses+" " + totalIncome.populationExpenses);
        return totalIncome;
    }
/// <summary>
/// subtracts people from everytile evenly and randomly
/// </summary>
/// <param name="subtractionAmt"></param>
    public void subtractPopulation(float subtractionAmt)
    {
        List<tileInfo> canidates = new List<tileInfo>();

        int tileAmt = allTilesOwned.Count;
        int canidateAmt = tileAmt;
       
        float amtToTakeCal(float pop, int tileAmountt) => (int)(pop / tileAmountt);
        float amtTotake = amtToTakeCal(subtractionAmt, tileAmt);


        for (int i = 0; i < tileAmt; i++)
        {
            tileInfo currentTIleInfo = allTilesOwned[i].GetComponent<tileInfo>();
            float acceptableAmt = currentTIleInfo.population * 0.6f;
            if (amtTotake > acceptableAmt)
            {

                canidateAmt -= 1;
                amtTotake = amtToTakeCal(subtractionAmt, canidateAmt);
                canidates.Clear();
                i = 0;

            }
            else
            {
                canidates.Add(currentTIleInfo);

            }


        }
        amtTotake = amtToTakeCal(subtractionAmt, canidateAmt);
        foreach(tileInfo currentCanidate in canidates)
        {
            currentCanidate.population -= amtTotake;
        }
    }
    


        
    
    
    

    


    




}


