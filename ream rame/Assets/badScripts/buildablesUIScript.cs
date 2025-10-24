
using UnityEngine;

public class buildablesUIScript : MonoBehaviour
{
    public popUpUIScript thisUIScript;
    
   

    public GameObject contentArea;


    // Update is called once per frame
    void Update()
    {
        buildableData skibid;
        skibid.money = 3f;
        
    }
}
public struct buildableData
{
    public float money;
}