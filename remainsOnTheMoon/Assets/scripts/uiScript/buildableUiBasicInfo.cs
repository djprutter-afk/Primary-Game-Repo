using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class buildableUiBasicInfo : MonoBehaviour
{
   
    public List<buildableScript> buildablesSelected = new List<buildableScript>();
    public GameObject quantityTextGameObject;
    public GameObject nameTextGameObject;
    public GameObject peopleTextGameObject;
    public GameObject resourceTextGameObject;
    public GameObject moneyTextGameObject;

    //List<> actionButtonsComponents;
    ////////////////////////////////////////////

    void Start()
    {
        
    }

    public void createUI()
    {
        

    }



    public void updateActionsUI(int quantity, string nameOfBuildable,BuildingStruct Upkeep)
    {
        
       
        TMP_Text quantityText = quantityTextGameObject.GetComponent<TMP_Text>();
 
        TMP_Text nameText = nameTextGameObject.GetComponent<TMP_Text>();
 
        TMP_Text peopleText = peopleTextGameObject.GetComponent<TMP_Text>();
  
        TMP_Text resourceText = resourceTextGameObject.GetComponent<TMP_Text>();

        TMP_Text moneyText = moneyTextGameObject.GetComponent<TMP_Text>();

        quantityText.text = quantity + "x";
        nameText.text = nameOfBuildable;
        peopleText.text = numericUtils.numberShortener(Upkeep.populationExpenses * quantity);
        resourceText.text = numericUtils.numberShortener(Upkeep.resourceExpenses * quantity);
        moneyText.text = numericUtils.numberShortener(Upkeep.moneyExpenses * quantity);

    }
}
