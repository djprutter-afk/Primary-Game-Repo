using UnityEngine;
using TMPro;

using System.Collections.Generic;
using System.Linq;

public class buildableButtonScript : MonoBehaviour
{
    public bool isBuilding;
    public colonyScript thisColony;
    [SerializeField] GameObject moneyExpenseText;
    [SerializeField] GameObject populationExpenseText;
    [SerializeField] GameObject resourceExpenseText;
    [SerializeField] GameObject buttonTextObject;
    public TMP_Text buttonText;

    public buildablesUIScript buildablescript;

    
    public popUpUIScript uiScript;
    public buildableGameObject thisBuildable;

    public string nameOfBuildable;
    TriValueStruct buildableCost;
    void Start()
    {
         uiScript = buildablescript.thisUIScript;
       
    }




    public void updateBuildable()
    {



        buttonText = buttonTextObject.GetComponent<TMP_Text>();
        buttonText.text = nameOfBuildable;
        TMP_Text moneyText = moneyExpenseText.GetComponent<TMP_Text>();
        TMP_Text popText = populationExpenseText.GetComponent<TMP_Text>();
        TMP_Text resourcetext = resourceExpenseText.GetComponent<TMP_Text>();
        moneyText.text = numericUtils.numberShortener(thisBuildable.buildCost.moneyValue);
        popText.text = numericUtils.numberShortener(thisBuildable.buildCost.populationValue);
        resourcetext.text =numericUtils.numberShortener( thisBuildable.buildCost.resourceValue);



    }

    public void buildBuilding()
    { 
         buildableCost = new TriValueStruct
        {
            moneyValue = thisBuildable.buildCost.moneyValue,
            populationValue = thisBuildable.buildCost.populationValue,
            resourceValue = thisBuildable.buildCost.resourceValue
        };
        int tileSelectedAmt = uiScript.tileselected.Length;
        int[] randomOrder = randomAssortment(tileSelectedAmt);



        for (int i = 0; i < tileSelectedAmt; i++)
        {
            Debug.Log(randomOrder.ToString());
            
            GameObject currentTile = uiScript.tileselected[randomOrder[i]];
            bool successInBuy = colonyMethoods.purchasableAction(thisColony.gameObject,buildableCost,currentTile, true);
            if (successInBuy == true)
            {
                tileInfo currentTileInfo = currentTile.GetComponent<tileInfo>();
                currentTileInfo.buildNewBuildable(thisBuildable, thisColony);


                /*

                GameObject newBuildable = Instantiate(thisBuildable.buildableObject,thisColony.gameObject.transform);
                Debug.Log(newBuildable);
                buildableScript newBuildableScript = newBuildable.GetComponent<buildableScript>();
                newBuildableScript.isBuilding = isBuilding;
                thisColony.ownedBuildables.Add(newBuildable);
                Debug.LogError(thisBuildable.isBuilding + " " + thisBuildable);
                newBuildableScript.tileOn = currentTile;
                newBuildableScript.becomeParellel();
                
                
                if (thisBuildable.isBuilding == true)
                {
                    tileInfo thisTileInfo = currentTile.GetComponent<tileInfo>();
               
                    thisTileInfo.buildingsOnTile.Add(newBuildableScript.upkeepCosts);
                    return;
                    
                }
                */
                

                
                


            }
        }
        
        


    }
    int[] randomAssortment(int length)
    {
        List<int> orderedRandomNummbers = Enumerable.Range(0, length).ToList();
        List<int> randomNummbers = new List<int>();




        for (int i = 0; i < length; i++)
        {
            int randIndex = Random.Range(0, orderedRandomNummbers.Count);
            randomNummbers.Add(orderedRandomNummbers[randIndex]);
            orderedRandomNummbers.RemoveAt(randIndex);

        }
        return randomNummbers.ToArray();


    }
    
 
}
