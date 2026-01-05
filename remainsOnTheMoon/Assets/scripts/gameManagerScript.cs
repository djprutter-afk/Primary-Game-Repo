using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class buildableGameObject//building
{

    public BuildingStruct buildCost;

    // thses expenses above are only buying the buildable NOT THE UPKEEP, UPKEEP IS STORED WITHIN THE BUILDABLESCRIPT ITSELF
    public GameObject buildableObject; // be sure to assign position
    public string nameOfBuildable;
    //public bool isBuilding;

}

[System.Serializable]
public class buildingCatagory//thing that contains buildings
{
    public string catagoryName;
    public Color catagoryColour;
    public buildableGameObject[] arrayOfBuildings;// building contained within the catagory

}

    class buildablesPurposesGrouped
    {


    public Dictionary<buildableScript.AIBuildableInfo.buildablePurposes, List<buildableGameObject>> purposesDictonary = new Dictionary<buildableScript.AIBuildableInfo.buildablePurposes, List<buildableGameObject>>();
        /// <summary>
        /// always null check this because it is initalized in OnEnable(), so it may not be ready when things call it
        /// </summary>
        public static Dictionary<buildableScript.AIBuildableInfo.buildablePurposes, List<buildableGameObject>> buildablePurposeDictonary;
       public  void skbidid()
        {

        foreach (buildingCatagory catagory in gameManagerScript.allCats)
        {
            foreach (buildableGameObject buildableGameObject in catagory.arrayOfBuildings)
            {
                buildableScript thisBuildableScript = buildableGameObject.buildableObject.GetComponent<buildableScript>();
                if (thisBuildableScript == null)
                {
                    continue;
                }
                foreach (buildableScript.AIBuildableInfo.biInfoStuct thisPurposes in thisBuildableScript.purposes)
                {
                    Debug.Log(thisPurposes.purpose + " HELP " + thisBuildableScript.purposes.Count());
                    if (purposesDictonary.ContainsKey(thisPurposes.purpose))
                    {
                        purposesDictonary[thisPurposes.purpose].Add(buildableGameObject);


                    }
                    else
                    {
                        purposesDictonary.Add(thisPurposes.purpose, new List<buildableGameObject>());
                        purposesDictonary[thisPurposes.purpose].Add(buildableGameObject);

                    }

                }
            }

        }
        buildablePurposeDictonary = purposesDictonary;
                
            
            
        }


    }
    /// <summary>
    ///  manufactures and sets up important stuff
    /// </summary>
public class gameManagerScript : MonoBehaviour
{
    public static event Action GameTick;
    public static event System.Action<BuildingStruct> onUpdate;
    [SerializeField] GameObject coloniesObject;
    gameSetup1 gamesetuper;
    [SerializeField] float gameTicketLength;
    [SerializeField] GameObject uiBuildablePrefab;
    [SerializeField] GameObject uiBuildableItemCatagoryPrefab;
    [SerializeField] GameObject uiBuildableItemPrefab;
    public static GameObject baseBuildableUI;


   


    public buildingCatagory[] allCatagories;

    public static buildingCatagory[] allCats;

    




    public GameObject colonies;

    
    colonyScript playercolonysscript;



    [Header("global buildiables")]
    [SerializeField] GameObject DevelopmentGameObject;
 
    public static buildableGameObject developmentGameObject;// sloppy

    void OnEnable()
    {

       
        developmentGameObject = new buildableGameObject();
        developmentGameObject.buildCost.moneyExpenses = 500;
        developmentGameObject.buildCost.resourceExpenses = 250;
        developmentGameObject.buildCost.populationExpenses = 50;
        developmentGameObject.buildableObject = DevelopmentGameObject;
        developmentGameObject.nameOfBuildable = "Development";

      
    }


    void Start()
    {
        allCats = allCatagories;
        buildablesPurposesGrouped newbuildablesPurposesGrouped = new buildablesPurposesGrouped();
        newbuildablesPurposesGrouped.skbidid();
        Debug.LogWarning(newbuildablesPurposesGrouped.purposesDictonary.Count + "see this please");
        
        
        SceneManager.LoadScene("uiScene", LoadSceneMode.Additive);

        


        InvokeRepeating("gameTick", 0, gameTicketLength);


        gamesetuper = coloniesObject.GetComponent<gameSetup1>();

      
        
        GameObject currentplayercolony = gamesetuper.playercolonies[0];
        playercolonysscript = currentplayercolony.GetComponent<colonyScript>();
        
        createBuildableUi();





       


    }
    void createBuildableUi()// terribly designed, should be remade
    {
        
        baseBuildableUI = Instantiate(uiBuildablePrefab);
        buildablesUIScript currentbuildableuiscript = baseBuildableUI.GetComponent<buildablesUIScript>();

        GameObject contentOfBuilldableUi = currentbuildableuiscript.contentArea;
        foreach (buildingCatagory currentCatagory in allCatagories)
        {
            GameObject currentCatagoryhere = Instantiate(uiBuildableItemCatagoryPrefab, contentOfBuilldableUi.transform);
            buildableCatagoryScript icantthinkofmorenames = currentCatagoryhere.GetComponent<buildableCatagoryScript>();
            icantthinkofmorenames.colourTheme = currentCatagory.catagoryColour;
            icantthinkofmorenames.catagoryTitle = currentCatagory.catagoryName;
            icantthinkofmorenames.builableScript = currentbuildableuiscript;
            
            foreach (buildableGameObject currentBuildable in currentCatagory.arrayOfBuildings)// the buttons within each catgory
            {
                GameObject buildingButton = Instantiate(uiBuildableItemPrefab, currentCatagoryhere.transform);
                buildableButtonScript currentBuildableScript = buildingButton.GetComponent<buildableButtonScript>();
                currentBuildableScript.thisBuildable.buildableObject = currentBuildable.buildableObject;
                currentBuildableScript.thisBuildable.buildCost.moneyExpenses = currentBuildable.buildCost.moneyExpenses;
                currentBuildableScript.thisBuildable.buildCost.populationExpenses = currentBuildable.buildCost.populationExpenses;
                currentBuildableScript.thisBuildable.buildCost.resourceExpenses = currentBuildable.buildCost.resourceExpenses;
                //currentBuildableScript.thisBuildable.isBuilding = currentBuildable.isBuilding;
                currentBuildableScript.nameOfBuildable = currentBuildable.nameOfBuildable;
                currentBuildableScript.thisColony = playercolonysscript;
                //currentBuildableScript.thisBuildable.isBuilding = currentBuildable.isBuilding;
                currentBuildableScript.buildablescript = currentbuildableuiscript;

                currentBuildableScript.updateBuildable();

            }

        }
    }
    public int ticksSinceStart;
/// <summary>
/// should be reworked
/// </summary>
    void gameTick()
    {
        
      ticksSinceStart += 1;
        GameTick?.Invoke();

        List<GameObject> allColonies = new List<GameObject>();

        for (int i = 0; i < colonies.transform.childCount; i++)
        {
            allColonies.Add(colonies.transform.GetChild(i).gameObject);

        }
        foreach (GameObject currentColony in allColonies)
        {


            colonyScript currentColonyScript = currentColony.GetComponent<colonyScript>();
            currentColonyScript.resourceUpdate();

            if (currentColony == playercolonysscript.gameObject)
            {
                BuildingStruct buildingStructArg = new BuildingStruct
                {
                    buildingName = playercolonysscript.gameObject.name,
                    moneyExpenses = playercolonysscript.resourcesOwned.moneyExpenses,
                    resourceExpenses = playercolonysscript.resourcesOwned.resourceExpenses,
                    populationExpenses = playercolonysscript.resourcesOwned.populationExpenses






                };
                onUpdate?.Invoke(buildingStructArg);

            }


        }
        


    }

    

}
