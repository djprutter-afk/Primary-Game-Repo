using Unity.VisualScripting;
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
            
            thisTileInfo.buildNewBuildable(gameManagerScript.developmentGameObject,ownercolonyscript);
            
            
            

            thistileVisuals.setupTileVisuals(ownercolonyscript.LOCALColonyMaterial,0.5f);
            ownercolonyscript.allTilesOwned.Add(thisTIle);
             BuildingStruct localadministration = new BuildingStruct
        {
           buildingName = "local admin building",
                moneyExpenses = 20,
                resourceExpenses = 20,
                populationExpenses = 0


        };


        //thisTileInfo.buildingsOnTile.Add(localadministration); // im removing line temporaraly readd if more balance is needed
        
        thisTileInfo.occupid = false;

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
