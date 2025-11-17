using UnityEngine;

public class settlerScript : MonoBehaviour
{
    buildableScript myBuildableScript;
    bool isSettleing = false;
    
    colonyScript ownercolonyscript;

    void Start()
    {
        myBuildableScript = GetComponent<buildableScript>();// buildable script will always be on the same GameObject so this is chill
        ownercolonyscript = transform.parent.GetComponent<colonyScript>();
        myBuildableScript.selfDies += onDeath;
        myBuildableScript.GenericAction += settleOnTile;
        myBuildableScript.doneMoving += settleTile;
        
        myBuildableScript.Move += settlerMovetoTile;



    }

    void onDeath()
    {
        Destroy(gameObject);

    }
    void settleTile()
    {
        GameObject thisTIle = myBuildableScript.tileOn;
        if (isSettleing == true)
        {
            thisTIle.transform.parent = transform.parent; // makes it so that tile becomes child of the owner of settler
            tileVisuals thistileVisuals = thisTIle.GetComponent<tileVisuals>();
            tileInfo thisTileInfo = thisTIle.GetComponent<tileInfo>();
            thisTileInfo.population = 50;
            thisTileInfo.development = 1;

            thistileVisuals.setupTileVisuals(ownercolonyscript.LOCALColonyMaterial);
            ownercolonyscript.allTilesOwned.Add(thisTIle);
             BuildingStruct localadministration = new BuildingStruct
        {
           buildingName = "local admin building",
                moneyExpenses = 20,
                resourceExpenses = 20,
                populationExpenses = 0


        };


        thisTileInfo.buildingsOnTile.Add(localadministration);

            Destroy(gameObject); // destroys the settler cause its like settled
        }

    }
    void settlerMovetoTile(GameObject tile)
    {
       
        myBuildableScript.moveToTileSetup(tile);
    }


  

    void settleOnTile(GameObject Tile)
    {
        
        myBuildableScript.moveToTileSetup(Tile);
        isSettleing = true;


    }
    
}
