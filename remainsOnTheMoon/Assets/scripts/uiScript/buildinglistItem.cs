using UnityEngine;
using TMPro;

public class buildinglistItem : MonoBehaviour
{

    [SerializeField] GameObject buildingNameTextObject;
    [SerializeField] GameObject moneyTextObject;
    [SerializeField] GameObject resourceTextObject;
    [SerializeField] GameObject populationTextObject;
    [SerializeField] GameObject quanitiyTextObject;


    public void updateBuldingList(BuildingStruct building, int quanitiy)
    {
        

        TMP_Text buildingNameText = buildingNameTextObject.GetComponent<TMP_Text>();
        TMP_Text moneyText = moneyTextObject.GetComponent<TMP_Text>();
        TMP_Text resourceText = resourceTextObject.GetComponent<TMP_Text>();
        TMP_Text popText = populationTextObject.GetComponent<TMP_Text>();
        TMP_Text quantText = quanitiyTextObject.GetComponent<TMP_Text>();
        TMP_Text[] alltexts = { moneyText, resourceText, popText };

        buildingNameText.text = building.buildingName;

        quantText.text = "x" + quanitiy.ToString();

        moneyText.text = "money " + numericUtils.numberShortener(building.moneyExpenses*quanitiy) + "$";
        resourceText.text = "resource " + numericUtils.numberShortener(building.resourceExpenses*quanitiy);
        popText.text = "pop " + numericUtils.numberShortener(building.populationExpenses*quanitiy);


    }


}
