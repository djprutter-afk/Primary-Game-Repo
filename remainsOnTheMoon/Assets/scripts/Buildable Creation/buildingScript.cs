using UnityEngine;

public class buildingScript : MonoBehaviour
{
     tileInfo tileonInfo;
     buildableScript myBuildableScript ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
         myBuildableScript = GetComponent<buildableScript>();
         tileonInfo = myBuildableScript.tileOn.GetComponent<tileInfo>();
        createBuilding();
    }
    public virtual void createBuilding()
    {
        
        tileonInfo.development += 1;
        Destroy(gameObject);
    }
}
// small test