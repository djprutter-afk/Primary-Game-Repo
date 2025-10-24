using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class buildableGameObject//building
{
    
    public float moneyExpenses;
    public float resourceExpenses;
    public int populationExpenses;
    public GameObject buildableObject; // be sure to assign position
    public string nameOfBuildable;
    public bool isBuilding;

}

[System.Serializable]
    public class buildingCatagory//thing that contains buildings
    {
        public string catagoryName;
        public Color catagoryColour;
        public buildableGameObject[] arrayOfBuildings;// building contained within the catagory

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


    public enum catagoriesEnum
    {
        dog
    
    }


    public buildingCatagory[] allCatagories;

    public static buildingCatagory[] allCats;

    




    public GameObject colonies;

    
    colonyScript playercolonysscript;

    




    void Start()
    {
        allCats = allCatagories;
        
        SceneManager.LoadScene("uiScene", LoadSceneMode.Additive);

        


        InvokeRepeating("gameTick", 0, gameTicketLength);


        gamesetuper = coloniesObject.GetComponent<gameSetup1>();

        Debug.LogError(gamesetuper.playercolonies.Count);
        
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
                currentBuildableScript.thisBuildable.moneyExpenses = currentBuildable.moneyExpenses;
                currentBuildableScript.thisBuildable.populationExpenses = currentBuildable.populationExpenses;
                currentBuildableScript.thisBuildable.resourceExpenses = currentBuildable.resourceExpenses;
                currentBuildableScript.thisBuildable.isBuilding = currentBuildable.isBuilding;
                currentBuildableScript.nameOfBuildable = currentBuildable.nameOfBuildable;
                currentBuildableScript.thisColony = playercolonysscript;
                currentBuildableScript.thisBuildable.isBuilding = currentBuildable.isBuilding;
                currentBuildableScript.buildablescript = currentbuildableuiscript;

                currentBuildableScript.updateBuildable();

            }

        }
    }

    void gameTick()
    {
        
      
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
        GameTick?.Invoke();



    }

    

}
