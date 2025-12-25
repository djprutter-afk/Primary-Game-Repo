using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.WSA;
using System.Collections.Generic;

public class buildableScript : MonoBehaviour
{
    public string nameOfBuildable; // must be assinged in editor for more info look in the buildableActionsScript


    
    public event System.Action finishedAction; // you should understand
    public event System.Action<GameObject> launch; // GameObject is target position
    public event System.Action<GameObject> Attack;//  GameObject is also target
    
    public event System.Action<GameObject> Move;
    public event System.Action Repair;
    public event System.Action CancelAll;// should cancel all actions if they are cancelable, uncancelable would be like a missile launch or something
    public event System.Action<GameObject> GenericAction; // for anything unit specfifc, GameObject is also target
    public event System.Action selfDies;

    public event System.Action doneCreatingSelf; // for when the buildable is ready




    public enum buildableActions
    {
        launch,
        Attack,
        Move,
        Repair,
        GenericAction,
        selfDies
    }





    float buildableHealth = 100f;// 100 health will be default but should be changed within unity inspector
    public event System.Action doneMoving;
    [SerializeField] Vector3 rotationOfBuildable;

    [SerializeField] bool currentlyClicked;
    public float possibleRangeDiameter;
    [SerializeField] GameObject rangeIndicator;
    public event System.Action<int> ClickedOn;
    public GameObject tileOn;
    public float unitFloorDistance = 1f;
    GameObject indicator;
    [Header("movement Setup")]
    [SerializeField] bool isMoving = false;
    bool isSliding;
    [SerializeField] float timeToWait = 1f;// time to spend on every tile when moving tile to tile to a target
    float currentTimeToNextMove; // when buildable moves it will wait this amount of time before moving again

    [SerializeField] GameObject[] movePath;
    int movePathPosition;
    GameObject endTargetTile;

    public BuildingStruct upkeepCosts;
    public buildableActions[] possibleActions; // actions that this buildable can perform, assinged in editor

    public bool isBuilding;
    colonyScript ownerColonyScript;



    //[Header("info")]
    [System.Serializable]
    public class AIBuildableInfo
    {
        [System.Serializable]
        public enum buildablePurposes
        {
            offensive,
            defensive,
            expansion,
            economy,
            suicidieOffensive,// missiles
            antiMissile

        }
        [System.Serializable]
        public struct biInfoStuct
        {
            public buildablePurposes purpose;
            public float strength; // how effective the builable is at this purpose 0% to 100%
            
        }

        
        
        

    }
    public AIBuildableInfo.biInfoStuct[] purposes;
    public bool buildableAction(buildableActions action, GameObject target)
    {
        Debug.Log(" attempting to work please please " + action);
        
        if (Vector3.Distance(target.transform.position, transform.position) > possibleRangeDiameter / 2)
        {
            Debug.LogError("FAILED FAILED, target was: " + target +" le object was also: " + gameObject);
            return false;
        }
      
        switch (action)
        {
            case buildableActions.launch:
                launch?.Invoke(target);
                break;
            case buildableActions.Attack:
                Attack?.Invoke(target);
                break;
            case buildableActions.Move:
                Move?.Invoke(target);
                break;
            case buildableActions.Repair:
                Repair?.Invoke();
                break;
            case buildableActions.GenericAction:
                GenericAction?.Invoke(target);
                
                break;
            case buildableActions.selfDies:
                selfDies?.Invoke();
                break;
           
            



        }
        return true;


    }

    public void takeDamage(float damageAmt)
    {
        buildableHealth -= damageAmt;
        if (buildableHealth <= 0)
        {
            tileInfo tileonInfo = tileOn.GetComponent<tileInfo>();
        tileonInfo.occupid = false;
            selfDies?.Invoke();

        }

    }




    public void deSelect()
    {
        currentlyClicked = false;
        indicator.SetActive(false);



    }

    void Start()
    {
        
        ownerColonyScript = transform.parent.GetComponent<colonyScript>();
        ownerColonyScript.colonyResorceUpdate += upkeepCostsToColony;
        indicator = Instantiate(rangeIndicator);

        indicator.transform.position = transform.position;
        indicator.transform.localScale = new Vector3(possibleRangeDiameter, possibleRangeDiameter, possibleRangeDiameter);
        indicator.transform.SetParent(transform);
        indicator.transform.position = transform.position;

        indicator.SetActive(false);


        tileInfo tileonInfo = tileOn.GetComponent<tileInfo>();
        if(isBuilding == false)
        {
            tileonInfo.occupid = true;
        }
        doneCreatingSelf?.Invoke();
        

    }

    public void clickByMouse(int mouseButtonDown, GameObject clicker)
    {
        if (transform.parent.gameObject != clicker)
        {
            return;
        }


        ClickedOn?.Invoke(mouseButtonDown);
        if (currentlyClicked == true)
        {

            currentlyClicked = false;
        }
        else
        {

            currentlyClicked = true;
        }
        indicator.SetActive(currentlyClicked);


    }


    public void becomeParellel()
    {
        transform.position = tileOn.transform.position;
        Vector3 center = new Vector3(0, 0, 0);
        transform.LookAt(center);

        transform.position = Vector3.MoveTowards(transform.position, center, -unitFloorDistance);
        transform.eulerAngles += rotationOfBuildable;



    }
    void Update()
    {
        if (isMoving == true)
        {
            currentTimeToNextMove -= Time.deltaTime;
            if (currentTimeToNextMove <= 0)
            {
               
                if (movePathPosition >= movePath.Count())
                {

                    doneMoving?.Invoke();
                    isMoving = false;
                    FinsihedAction();
                    return;
                }
                if(movePath[movePathPosition] == null)
                {
                    moveToTileSetup(endTargetTile);
                    return;
                }
                moveToTile(movePath[movePathPosition]);
                movePathPosition += 1;
            }


        }

    }
    public void moveToTile(GameObject tile)
    {
        tileInfo currentTileInfo = tileOn.GetComponent<tileInfo>();
        tileInfo nextTileInfo = tile.GetComponent<tileInfo>();
        if(currentTileInfo == null || nextTileInfo == null)
        {
            return;
        }
        if (nextTileInfo.occupid == true)
        {
            moveToTileSetup(endTargetTile);

            
        }
        currentTileInfo.occupid = false;
        nextTileInfo.occupid = true;
        tileOn = tile;
        becomeParellel();
        currentTimeToNextMove = timeToWait;
    }

    public void moveToTileSetup(GameObject endTarget)
    {

        movePath = colonyMethoods.pathtingAlgorthim(tileOn, endTarget).ToArray();
        if (movePath == null)
        {
            return;
        }
        movePathPosition = 0;
        endTargetTile = endTarget;
        isMoving = true;
    }
    
    public void FinsihedAction()
    {

        finishedAction?.Invoke();
    }

    void upkeepCostsToColony()
    {
        if (isBuilding == true)
        {
            return;
        }
        ownerColonyScript.resourcesOwned.moneyExpenses -= upkeepCosts.moneyExpenses;
        ownerColonyScript.resourcesOwned.resourceExpenses -= upkeepCosts.resourceExpenses;
        ownerColonyScript.subtractPopulation(upkeepCosts.populationExpenses);// has to be done differently because the population must from the tiles, the colonyscript "population" is just the sum of all tile's population and changing it does nothing





    }


    void OnDestroy()
    {
        ownerColonyScript.ownedBuildables.Remove(gameObject);

    }
    
}
