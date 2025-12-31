using UnityEngine;
using TMPro;

using System.Collections.Generic;

using System.Linq;
public class popUpUIScript : MonoBehaviour// this script is for the tile popups
{
    Dictionary<BuildingStruct, int> tilesBuildingsDic = new Dictionary<BuildingStruct, int>();
    public GameObject[] tileselected;// this is the tile or tiles we getting info about
    [SerializeField] GameObject buildingList;
    [SerializeField] GameObject buidablesUI;
    [SerializeField] GameObject buildingListContent;
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text resourceText;
    [SerializeField] TMP_Text popText;
    [SerializeField] GameObject buildinglistItem;
    [SerializeField] GameObject exitButton;

    

    public float totalIncome = 0; // money income
    public float totalResources = 0;
    public float totalpopulation = 0;
    public float totalDevelopment = 0;
    // total of all selected tiles, probs should of though of a better name

    void Start()
    {
        Transform childCanvas = transform.GetChild(0);
        GameObject sigma = Instantiate(gameManagerScript.baseBuildableUI, childCanvas);
        buildablesUIScript alpha = sigma.GetComponent<buildablesUIScript>();

        alpha.thisUIScript = this;
        transform.forward = Camera.main.transform.forward;
        
        

        InvokeRepeating(nameof(updatePopup), 0f,5);
        
       

    }
    void OnDestroy()
    {
        CancelInvoke(nameof(updatePopup));
    }
    void Update()
    {
        transform.forward = Camera.main.transform.forward;

        
    }
    public void exitButtonPress()
    {
        Destroy(gameObject);
    }

    public void updatePopup()
    {


        if (tileselected.Length <= 0 || tileselected[0] == null)// if there isnt any tiles selected then do not update ui, if the first one is null then that means the array must be empty... probs
        {
            moneyText.text = "0";
            resourceText.text = "0";
            popText.text = "0";

            foreach (KeyValuePair<BuildingStruct, int> tileDicKvp in tilesBuildingsDic.ToList())// set every value to zero becuase the total amount of every item is going to added
            {


                tilesBuildingsDic[tileDicKvp.Key] = 0;
            }




            return;
        }
        if (buildingList.activeSelf == true)
        {
            foreach (KeyValuePair<BuildingStruct, int> tileDicKvp in tilesBuildingsDic.ToList())// set every value to zero becuase the total amount of every item is going to added
            {

                tilesBuildingsDic[tileDicKvp.Key] = 0;
            }



        }



        totalDevelopment = 0;
        totalIncome = 0; // money income
        totalResources = 0;
        totalpopulation = 0;



        foreach (GameObject currentTile in tileselected)
        {
            if (currentTile == null)
            {
                return;
            }
            tileInfo currentTileInfo = currentTile.GetComponent<tileInfo>();
            if (currentTileInfo == null)
            {


                return;

            }

            BuildingStruct tileTotal = currentTileInfo.TotalIncome();
            totalDevelopment += currentTileInfo.development;

            totalIncome += tileTotal.moneyExpenses;

            totalResources += tileTotal.resourceExpenses;

            totalpopulation += currentTileInfo.population;

            if (buildingList.activeSelf == true)// all this code nested needs a check over
            {
                foreach (BuildingStruct building in currentTileInfo.buildingsOnTile)
                {
                    if (tilesBuildingsDic.ContainsKey(building) == false)
                    {

                        tilesBuildingsDic.Add(building, 1);
                    }
                    else
                    {

                        tilesBuildingsDic[building] += 1; // if building already exist in dictonary then add 1 to its value
                    }
                }
            }
        }

        Transform[] allBuildingItems = buildingListContent.GetComponentsInChildren<Transform>();



        foreach (KeyValuePair<BuildingStruct, int> tileDicKvp in tilesBuildingsDic)// iterates over every building that should be on 
        {
            if (tilesBuildingsDic[tileDicKvp.Key] <= 0)
            {
                foreach (Transform child in allBuildingItems)
                {
                    if (child.name == tileDicKvp.Key.buildingName)
                    {
                        Destroy(child); // destroys the item that has 0 quanitiy
                    }
                }

                tilesBuildingsDic.Remove(tileDicKvp.Key);

            }







            bool isRepresented = false;
            GameObject representedObject = null;
            for (int i = 0; i < allBuildingItems.Length; i++)// checks all current building items if current tiledickKvp is represented
            {
                if (allBuildingItems[i].name == tileDicKvp.Key.buildingName)
                {
                    isRepresented = true;
                    representedObject = allBuildingItems[i].gameObject;

                }
            }


            if (isRepresented == false)
            {
                GameObject createdObject = Instantiate(buildinglistItem, buildingListContent.transform);
                buildinglistItem itemScript = createdObject.GetComponent<buildinglistItem>();




                itemScript.updateBuldingList(tileDicKvp.Key, tileDicKvp.Value);

                createdObject.name = tileDicKvp.Key.buildingName;

            }
            else if (isRepresented == true && representedObject != null)
            {
                buildinglistItem itemScript = representedObject.GetComponent<buildinglistItem>();

                itemScript.updateBuldingList(tileDicKvp.Key, tileDicKvp.Value);


            }



        }








        moneyText.text = numericUtils.numberShortener(totalIncome);
        resourceText.text = numericUtils.numberShortener(totalResources) ;
        popText.text = numericUtils.numberShortener(totalpopulation)+ "/" + numericUtils.numberShortener(totalDevelopment * 250);



        //buildables update

       // colonyMethoods.purchasableAction






    }

    public void toggleBuildingList()
    {
        if (buildingList.activeSelf)
        {
            buildingList.SetActive(false);



        }
        else
        {
            buildingList.SetActive(true);
        }

    }

    
    
}
