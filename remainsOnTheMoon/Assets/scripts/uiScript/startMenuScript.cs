using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class startMenuScript : MonoBehaviour
{
    [SerializeField] Scene mainGame;
    
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject CreditButton;
    [SerializeField] GameObject SettingButton;
   public void startPress()
    {
        
    }
     public void SettingsPress()
    {
        
    }

 public void exitPress()
    {
        Application.Quit();
    }
}

