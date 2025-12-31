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
    [SerializeField] GameObject launchButtonObject;
    TMP_Text moneyText;
    TMP_Text resourceText;
    TMP_Text populationText;

    List<TMP_Text> textUI;

   

    void Start()
    {




        moneyText = moneyTextObject.GetComponent<TMP_Text>();

        resourceText = resourceTextObject.GetComponent<TMP_Text>();

        populationText = populationTextObject.GetComponent<TMP_Text>();



        gameManagerScript.onUpdate += updateText;
    }
    void OnDisable()
    {
        gameManagerScript.onUpdate -= updateText;
    }
    public void launchButtonPress()
    {
        Debug.Log("EVERYTHING DOESN WORK");
        lunachbuttonPress?.Invoke(true);

    }




    void updateText(BuildingStruct currentValues)
    {
        moneyText.text = numericUtils.numberShortener(currentValues.moneyExpenses);
        resourceText.text = numericUtils.numberShortener(currentValues.resourceExpenses);
        populationText.text = numericUtils.numberShortener(currentValues.populationExpenses);




    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }
    }

    void resizeSelectionBOx()
    {
        
    }
}
