using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class uiManagerScript : MonoBehaviour
{
    public static event System.Action<bool> lunachbuttonPress;
    [SerializeField] GameObject moneyTextObject;
    [SerializeField] GameObject resourceTextObject;
    [SerializeField] GameObject populationTextObject;
    [SerializeField] GameObject incomeMoneyTextObject;
    [SerializeField] GameObject incomeResourceTextObject;
    [SerializeField] GameObject incomePopulationTextObject;
    TMP_Text moneyText;
    TMP_Text resourceText;
    TMP_Text populationText;
    TMP_Text incomeMoneyText;
    TMP_Text incomeResourceText;
    TMP_Text incomePopulationText;

    List<TMP_Text> textUI;

   

    void Start()
    {




        moneyText = moneyTextObject.GetComponent<TMP_Text>();
        resourceText = resourceTextObject.GetComponent<TMP_Text>();
        populationText = populationTextObject.GetComponent<TMP_Text>();
        
        incomeMoneyText = incomeMoneyTextObject.GetComponent<TMP_Text>();
        incomeResourceText = incomeResourceTextObject.GetComponent<TMP_Text>();
        incomePopulationText = incomePopulationTextObject.GetComponent<TMP_Text>();




        gameManagerScript.onUpdate += updateText;
    }
    void OnDisable()
    {
        gameManagerScript.onUpdate -= updateText;
    }
    BuildingStruct previousIncome = new BuildingStruct();




    void updateText(BuildingStruct currentValues)
    {
        moneyText.text = numericUtils.numberShortener(currentValues.moneyExpenses);
        resourceText.text = numericUtils.numberShortener(currentValues.resourceExpenses);
        populationText.text = numericUtils.numberShortener(currentValues.populationExpenses);

        BuildingStruct change = currentValues.subtract(previousIncome);

        incomeMoneyText.text = numericUtils.numberShortener(change.moneyExpenses);
        incomeResourceText.text = numericUtils.numberShortener(change.resourceExpenses);
        incomePopulationText.text = numericUtils.numberShortener(change.populationExpenses);




    }

}
