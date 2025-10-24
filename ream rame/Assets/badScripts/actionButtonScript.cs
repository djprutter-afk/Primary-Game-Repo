using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class actionButtonScript : MonoBehaviour
{
    public static event Action<buildableScript.buildableActions> performActionPMI;
    public GameObject textOnButton;
    
    

    public buildableScript.buildableActions ActionToPerform;
    buildableActionsItem parentScript;


    void Start()
    {
        parentScript = transform.parent.GetComponent<buildableActionsItem>();
    }





    public void thisButtonClick()
    {

        //List<buildableScript> selectedUnits = parentScript.buildablesSelected;
        performActionPMI?.Invoke(ActionToPerform);



    }
  
}
