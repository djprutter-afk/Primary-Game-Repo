using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.EventSystems;

public class playerMouseInteractions : MonoBehaviour// for the sole player only
{
    public GameObject playerColony;
    [SerializeField] float effectDuration;
    public LayerMask interactionLayerMask;
    public GameObject mainCamera;
    Camera mainCameraComponent;

    [SerializeField] GameObject tilePopupUI;
    [SerializeField] Material selecteMaterial;

    public Material fadeMaterial;

    [SerializeField] List<GameObject> currentTileSelection = new List<GameObject>(); // only seralizedfield to debug from editor, USED NOWERE ELSE
    public List<GameObject> currentBuildableSelection = new List<GameObject>();
    public static event System.Action<List<GameObject>> buildableSelectionUpdate;
    colonyScript playerColonyScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        mainCameraComponent = mainCamera.GetComponent<Camera>();
        actionButtonScript.performActionPMI += launchUnits;

     




    }
   



    // Update is called once per frame
    void Update()
    {
        if (playerColony != null && playerColonyScript == null)
        {
            playerColonyScript = playerColony.GetComponent<colonyScript>();
        }
        Ray cameraRay = mainCameraComponent.ScreenPointToRay(Input.mousePosition);



        if (Physics.Raycast(cameraRay, out RaycastHit hitObject, 15, interactionLayerMask))
        {
            bool mouseOverNotUi = EventSystem.current.IsPointerOverGameObject();
            tileVisuals currentTilVisual = hitObject.transform.GetComponent<tileVisuals>();


            if (hitObject.transform.tag == "unit" && Input.GetMouseButtonDown(0))
            {
                buildableScript thisUnit = hitObject.transform.GetComponent<buildableScript>();
                thisUnit.clickByMouse(0, playerColony);
                if (currentBuildableSelection.Contains(thisUnit.gameObject))
                {
                    currentBuildableSelection.Remove(thisUnit.gameObject);
                }
                else
                {
                    currentBuildableSelection.Add(thisUnit.gameObject);

                }
                buildableSelectionUpdate?.Invoke(currentBuildableSelection);
            }

            if (currentTilVisual != null && mouseOverNotUi == false)// means object hit is a tile because ONLY tiles will have the "tileVisuals" script attached
            {


                currentTilVisual.fadeEffect(effectDuration, fadeMaterial);

                if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
                {
                    if (currentTileSelection.Contains(hitObject.transform.gameObject) == false)
                    {
                        tileVisuals metileviusal = hitObject.transform.gameObject.GetComponent<tileVisuals>();
                        metileviusal.Select(selecteMaterial);
                        currentTileSelection.Add(hitObject.transform.gameObject);
                    }

                }


                if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) == false)// if left click ever happens on a tile or selection of tiles then a new tile UI popup will be created
                {






                    bool isBoardering = false;
                    if (currentTileSelection.Contains(hitObject.transform.gameObject) == false)
                    {
                        currentTileSelection.Add(hitObject.transform.gameObject);
                        
                        
                        
                    }
                    Vector3 spawnLocation = CenterOfPositions(currentTileSelection.ToArray());
                    GameObject newTilePopUp = Instantiate(tilePopupUI, spawnLocation, transform.rotation);
                    //colonyMethoods.findEdgeTiles(playerColony);
                    foreach (GameObject tile in currentTileSelection)
                    {
                        if (playerColonyScript.outerUnownedTiles.Contains(tile) == true)
                        {
                            isBoardering = true;// todo finsih the "building" script ... finsihed idk what i ment
                        }

                    }

                    popUpUIScript popUpScript = newTilePopUp.GetComponent<popUpUIScript>();
                    popUpScript.tileselected = currentTileSelection.ToArray();

                    massDeselectTiles();


                }

            }
            else
            {
                //if (Input.GetMouseButtonDown(0))// means player is clicking on somthing that isnt a tile
                //{
                //    massDeselectTiles();
                //}
            }











        }
    



    }

    public void launchUnits(buildableScript.buildableActions Action)
    {
     



        int buildableAmt = currentBuildableSelection.Count;
        int tileSlectecAmt = currentTileSelection.Count;
        if (buildableAmt < 0 || tileSlectecAmt < 0)
        {
            Debug.Log("launching fauked");
            return;
        }

        int[] randomOrder = randomAssortment(tileSlectecAmt);

        int tileIndex = 0;
        for (int i = 0; i < buildableAmt; i++)
        {

            if (tileIndex >= tileSlectecAmt)// if there are more buildables than tiles then loop to the first tile
            {
                tileIndex = 0;
            }
            buildableScript currentBuildable = currentBuildableSelection[i].GetComponent<buildableScript>();

          
            bool suceeded = currentBuildable.buildableAction(Action, currentTileSelection[randomOrder[tileIndex]]);

            if (suceeded == false)
            {
                foreach (GameObject tile in currentTileSelection)// if above fails then check every tile for validness 
                {
                    bool sucess = currentBuildable.buildableAction(Action, tile);
                    if (sucess == true)
                    {
                        break;
                    }

                }
            }

            tileIndex += 1;








        }


        massDeselectBuildables();
        massDeselectTiles();
        

    }

    Vector3 CenterOfPositions(GameObject[] objects)
    {
        Vector3 centerOfObjects = Vector3.zero;
        foreach (GameObject currentObject in objects)
        {
            centerOfObjects += currentObject.transform.position;

        }



        int longestDis = 0;
        for (int i = 0; i < 3; i++)
        {
            float distance = centerOfObjects[i] /= objects.Length;
            float absValue = math.abs(distance);

            if (math.abs(centerOfObjects[longestDis]) < absValue)
            {
                longestDis = i;
            }



        }
        if (math.abs(centerOfObjects[longestDis]) < 2.05f)
        {
            if (centerOfObjects[longestDis] > 0)
            {
                centerOfObjects[longestDis] = 2.05f; // makess it so that the 
            }
            else
            {
                centerOfObjects[longestDis] = -2.05f; // makess it so that the 
            }




        }







        return centerOfObjects;



    }

    void massDeselectBuildables()
    {
        foreach (GameObject currentBuildable in currentBuildableSelection)
        {
            buildableScript currentBuildableScipt = currentBuildable.GetComponent<buildableScript>();
            currentBuildableScipt.deSelect();
        }
        currentBuildableSelection.Clear();
        buildableSelectionUpdate?.Invoke(currentBuildableSelection);

    }
    void massDeselectTiles()
    {
        
        foreach (GameObject currentTile in currentTileSelection)
        {
            tileVisuals currentTileVisualScipt = currentTile.GetComponent<tileVisuals>();
            currentTileVisualScipt.deSelect();
        }
        currentTileSelection.Clear();
        

    }
    
      int[] randomAssortment(int length)
    {
        string debugNumbers = "";
        List<int> orderedRandomNummbers = Enumerable.Range(0, length).ToList();
        List<int> randomNummbers = new List<int>();




        for (int i = 0; i < length; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, orderedRandomNummbers.Count);
            randomNummbers.Add(orderedRandomNummbers[randIndex]);
            debugNumbers = debugNumbers +" "+orderedRandomNummbers[randIndex].ToString() ;
            orderedRandomNummbers.RemoveAt(randIndex);

        }
        Debug.Log(debugNumbers);
        return randomNummbers.ToArray();


    }
    
}
