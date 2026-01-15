using UnityEngine;

public class cloneVatScript : buildingScript
{
  
    public override void createBuilding()
    {
        Destroy(gameObject);
    }


}
