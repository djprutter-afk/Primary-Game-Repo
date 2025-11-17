using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class buildableActionsScript : MonoBehaviour
{

   
    //public List<GameObject> UIItems = new List<GameObject>();
    Dictionary<string, buildableUiBasicInfo> UIItems = new Dictionary<string, buildableUiBasicInfo>();// string is the name of buildable buildableactionsItem is the ui element
    List<string> checkedAlready = new List<string>();
    [SerializeField] GameObject basicInfo;
    [SerializeField] GameObject actionItem; // parent of all buttons
    [SerializeField] GameObject buttonBase; // le button
    void Start()
    {
        playerMouseInteractions.buildableSelectionUpdate += updateBuildableActionsUI;
    }
    public void updateBuildableActionsUI(List<GameObject> AllBuildables)// contains all buildables selected
    {
        List<string> UIElementsToRemoveNAME = new List<string>();
        checkedAlready.Clear();
        foreach (string nameKey in UIItems.Keys)
        {
            bool shouldBeOnUI = false;
            foreach (GameObject currentSelected in AllBuildables)
            {
                buildableScript currentBuildable = currentSelected.GetComponent<buildableScript>();
                string elNameOfBuildable = currentBuildable.nameOfBuildable;
                if (elNameOfBuildable == nameKey)
                {
                    shouldBeOnUI = true;
                    break;
                }

            }
            if (shouldBeOnUI == false)
            {

                UIElementsToRemoveNAME.Add(nameKey);
                


            }
        }
        foreach (string uiToremoveString in UIElementsToRemoveNAME)
        {
            GameObject UIToREMOVE = UIItems[uiToremoveString].gameObject;

            Destroy(UIToREMOVE.transform.parent.gameObject);
            UIItems.Remove(uiToremoveString);
        }
        UIElementsToRemoveNAME.Clear();
        
        foreach (GameObject buildableSelected in AllBuildables)
        {
            buildableScript currentBuildable = buildableSelected.GetComponent<buildableScript>();
            string elNameOfBuildable = currentBuildable.nameOfBuildable;

            if (checkedAlready.Contains(elNameOfBuildable) == true)
            {
                continue;// if buildable already check then SKIP

            }

            if (UIItems.ContainsKey(elNameOfBuildable) == true)
            {
                int amt = AmountOfItem(elNameOfBuildable, AllBuildables.ToArray());
                UIItems[elNameOfBuildable].updateActionsUI(amt, elNameOfBuildable, currentBuildable.upkeepCosts);
            }
            else if (UIItems.ContainsKey(elNameOfBuildable) == false)
            {
                GameObject createdUIItem = Instantiate(actionItem, transform);

                GameObject basicActionInfoGMOBJT = Instantiate(basicInfo, createdUIItem.transform);
                foreach (buildableScript.buildableActions acjtion in currentBuildable.possibleActions)// create buttons
                {
                    GameObject createdButton = Instantiate(buttonBase, createdUIItem.transform);
                    actionButtonScript currentButtonScript = createdButton.GetComponent<actionButtonScript>();
                    TMP_Text currenttextOnButton = currentButtonScript.textOnButton.GetComponent<TMP_Text>();
                    string actionText = acjtion.ToString();
                    currenttextOnButton.text = actionText;
                    currentButtonScript.ActionToPerform = acjtion;

                }

                buildableUiBasicInfo skidib = basicActionInfoGMOBJT.GetComponent<buildableUiBasicInfo>();

                int amt2 = AmountOfItem(elNameOfBuildable, AllBuildables.ToArray());
                skidib.updateActionsUI(amt2, currentBuildable.nameOfBuildable, currentBuildable.upkeepCosts);
                UIItems.Add(elNameOfBuildable, skidib);

            }

        }
        
        
       








    }

    int AmountOfItem(string itemtoCheck, GameObject[] allitems)
    {
        int total = 0;
        foreach (GameObject currentObject in allitems)
        {
            buildableScript currentBuildable = currentObject.GetComponent<buildableScript>();
            if (currentBuildable.nameOfBuildable == itemtoCheck)
            {
                total += 1;


            }

        }
        checkedAlready.Add(itemtoCheck);
        return total;
    }
    
}
