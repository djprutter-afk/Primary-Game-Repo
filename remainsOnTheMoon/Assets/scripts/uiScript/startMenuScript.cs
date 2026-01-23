using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
public class startMenuScript : MonoBehaviour
{
    [SerializeField] Scene mainGame;
    
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject CreditButton;
    [SerializeField] GameObject SettingButton;
     [SerializeField] GameObject helpButtonTextWall;
     [SerializeField] TMP_Text tMP_Text;
    void Start()
    {
         helpButtonTextWall.SetActive(false);
    }
    public void startPress()
    {
       
      SceneManager.LoadScene(1);
    }
     public void SettingsPress()
    {
        helpButtonTextWall.SetActive(helpButtonTextWall.activeSelf == false);
        
    }

 public void exitPress()
    {
        Application.Quit();
    }
    int timesPress = 0;
     public void clipBoardButtonPress()
    { timesPress++;
        GUIUtility.systemCopyBuffer ="https://github.com/djprutter-afk/Primary-Game-Repo";
        switch(timesPress)
        {
            case 1:
            tMP_Text.text = "Copied!";
            break;
             case 2:
            tMP_Text.text = "Super copied!";
            break;
             case 3:
            tMP_Text.text = "Ultra copied!";
            break;
             case 4:
            tMP_Text.text = "Mega copied!";
            break;
             case 5:
            tMP_Text.text = "Giga Copied!";
            break;
            case 6:
            tMP_Text.text = "Too many copied!";
            break;
             case 7:
            tMP_Text.text = "WAY TOO MANY COPIED!";
            break;
             default:
            tMP_Text.text = "ok you can stop now";
            break;

        }
            
        
    }
}

