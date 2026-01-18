
using System.Collections.Generic;
using UnityEngine;

public class gameSetup1 : MonoBehaviour// poorly named should be colonysetup as vital game setup functions have been spread across multiple scripts now
{
    [SerializeField] Material allColonyMaterial;
    public GameObject playerColonyPrefab;

    public List<GameObject> playercolonies;
    [SerializeField] int playerAmount;
    public float minDistance;
    public GameObject theMoon;
    public int colonyAmount;
    public GameObject gameManager;

    public GameObject playerMouseInteractionsObject;

    playerMouseInteractions thePlayerMouseInteractionsInator;
  

    

    [Header("All posssible colony types")]
    public GameObject[] colonyTypes;
    GameObject[] colonyPositions;

    int totalColoniesPlaced;

    List<colonyScript> allColonieScripts = new List<colonyScript>();
    public static TriValueStruct avergeResourceAmt;

    

   




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        AIMultithreader aIMultithreader = GetComponent<AIMultithreader>();
        
        thePlayerMouseInteractionsInator = playerMouseInteractionsObject.GetComponent<playerMouseInteractions>();
        totalColoniesPlaced = 0;
        colonyPositions = new GameObject[colonyAmount];
        int playerColoniesPlaced = 0;

        
        
        


        for (int i = 0; i < colonyAmount; i++)
        {
            int moonChildren = theMoon.transform.childCount;
            GameObject placingColony;

            if (playerColoniesPlaced < playerAmount)
            {
                placingColony = playerColonyPrefab;
                playerColoniesPlaced++;

            }
            else // start creating ai colonies
            {
                int indexOfColonies = Random.Range(0, colonyTypes.Length);
                placingColony = colonyTypes[indexOfColonies];

            }
            
            GameObject createdColony = Instantiate(placingColony, Vector3.zero, transform.rotation, transform);
            if (placingColony == playerColonyPrefab)
            {
                thePlayerMouseInteractionsInator.playerColony = createdColony;
                playercolonies.Add(createdColony);
                
            }
         
            colonyStart colonyStartScript = createdColony.GetComponent<colonyStart>();
            colonyScript thisColonyScript = createdColony.GetComponent<colonyScript>();
            allColonieScripts.Add(thisColonyScript);
            thisColonyScript.globalColonyMaterial = allColonyMaterial;
            baseColonyAI AIAIAIAIAAIAIAIA = createdColony.GetComponent<baseColonyAI>();
            if (AIAIAIAIAAIAIAIA != null)
            {
                AIAIAIAIAAIAIAIA.theGameManager = gameManager;
                aIMultithreader.artificialIntelligences.Add(AIAIAIAIAAIAIAIA);
            }

            colonyScript creaetedColonyScript = createdColony.GetComponent<colonyScript>();
            creaetedColonyScript.colonyGameManager = gameManager;


            int randomPositionIndex = Random.Range(0, moonChildren);

            GameObject checkingColonyPosition = theMoon.transform.GetChild(randomPositionIndex).gameObject;// thus begin the games...



            int attempts = 0;
            while (attempts < 50)
            {





                if (i == 0) // only for first colony, cause like theree isnt any other colonies to compare positions :(
                {

                    colonyPositions[0] = checkingColonyPosition;
                    break;
                }


                if (distanceChecker(checkingColonyPosition) == false)
                {
                    attempts++;
                    continue;

                }
                else
                {
                    for (int k = 0; k < colonyPositions.Length; k++)
                    {

                        if (colonyPositions[k] == null)
                        {

                            colonyPositions[k] = checkingColonyPosition;

                            break;
                        }
                    }

                    break;
                }






            }





            if (attempts < 50)
            {
                totalColoniesPlaced += 1;



                colonyStartScript.colonyStartPosition = checkingColonyPosition;
                tileInfo thisTilesInfo = colonyStartScript.colonyStartPosition.GetComponent<tileInfo>();
                if (thisTilesInfo != null)
                {
                   
                       thisTilesInfo.buildNewBuildable(gameManagerScript.developmentGameObject, creaetedColonyScript);
                            thisTilesInfo.buildNewBuildable(gameManagerScript.developmentGameObject, creaetedColonyScript);
                    thisTilesInfo.population = 300;

                }



            }
            else
            {
                lastResort(createdColony,creaetedColonyScript);

            }


        }



    }

    void lastResort(GameObject failedColony,colonyScript FailedColonyScript)
    {
        int moonChildren = theMoon.transform.childCount;


        for (int i = 0; i < moonChildren; i++)
        {


            GameObject testObject = theMoon.transform.GetChild(i).gameObject;


            colonyStart colonyStartScript = failedColony.GetComponent<colonyStart>();
            if (distanceChecker(testObject) == true)
            {
                for (int k = 0; k < colonyPositions.Length; k++)
                {


                    if (colonyPositions[k] == null)
                    {
                        totalColoniesPlaced += 1;
                        colonyStartScript.colonyStartPosition = testObject;

                        tileInfo thisTilesInfo = colonyStartScript.colonyStartPosition.GetComponent<tileInfo>();
                        if (thisTilesInfo != null)
                        {


                            thisTilesInfo.buildNewBuildable(gameManagerScript.developmentGameObject, FailedColonyScript);
                            thisTilesInfo.buildNewBuildable(gameManagerScript.developmentGameObject, FailedColonyScript);

                            thisTilesInfo.population = 300;
                        }

                        colonyPositions[k] = testObject;
                        return;
                    }
                }




            }

        }
        Debug.LogError("last resort failed");
        colonyScript failedColonyScript = failedColony.GetComponent<colonyScript>();
        allColonieScripts.Remove(failedColonyScript);
    }






    bool distanceChecker(GameObject potentialColonyLocation)
    {
        for (int colonyIndex = 0; colonyIndex < totalColoniesPlaced; colonyIndex++)
        {



            if (Vector3.Distance(colonyPositions[colonyIndex].transform.position, potentialColonyLocation.transform.position) < minDistance)
            {

                return false; // false means ITSS BAD LOCATION!!!
            }

        }


        return true; // true mean positionis valid 

    }
    void Start()
    {
        gameManagerScript.GameTick += globalAveragesUpdate;
    }

    /// <summary>
    /// give average states about all colonies
    /// </summary>
    void globalAveragesUpdate()
    {
        TriValueStruct averages = new TriValueStruct();
        List<GameObject> markForDeath = new List<GameObject>();
        int colonyAmt = allColonieScripts.Count;
        foreach (colonyScript currentColonyScript in allColonieScripts)
        {
            if (currentColonyScript == null)
            {
                markForDeath.Add(currentColonyScript.gameObject);
                continue;

            }

            averages.moneyValue += currentColonyScript.resourcesOwned.moneyValue;
            averages.resourceValue += currentColonyScript.resourcesOwned.resourceValue;
            averages.populationValue += currentColonyScript.resourcesOwned.populationValue;

        }


        averages.moneyValue /= colonyAmt;
        averages.resourceValue /= colonyAmt;
        averages.populationValue /= colonyAmt;
        avergeResourceAmt = averages;

    }
    

 
}
