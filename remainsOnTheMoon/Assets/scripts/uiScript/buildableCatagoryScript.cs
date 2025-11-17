using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class buildableCatagoryScript : MonoBehaviour
{
  
    public Color colourTheme;
  
    public string catagoryTitle;
    [SerializeField] GameObject catagoryTitleObject;

    public buildablesUIScript builableScript; // refers to the buildables ui

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
     
        Image image = GetComponent<Image>();
        image.color = colourTheme;
        TMP_Text catagroyTitleComponent = catagoryTitleObject.GetComponent<TMP_Text>();

        catagroyTitleComponent.text = catagoryTitle;
        catagroyTitleComponent.color = Color.Lerp(Color.black, colourTheme, 0.15f);
         
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
