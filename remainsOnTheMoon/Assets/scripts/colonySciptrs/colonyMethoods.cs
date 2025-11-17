

using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.Collections;
using System.Collections.Specialized;
using System.Reflection.Emit;

/*
goals for this script:

make methods as self contained as possible

simply every possible thing, no matter how small (mostly)

be more useful than brain destroying ( looking back at this, it was almost certinally more brain destroying)

this is mostly for ai 




*/
public static class colonyMethoods
{
    public static GameObject[] findEdgeTiles(GameObject colony)
    {
         colonyScript currentColonyScript = colony.GetComponent<colonyScript>();
        GameObject[] ownedTiles = currentColonyScript.allTilesOwned.ToArray();
        List<GameObject> edgeTiles = new List<GameObject>();

        currentColonyScript.ownedEdgeTiles.Clear();

        foreach (GameObject tile in ownedTiles)
        {
            Collider[] surroundingTiles = Physics.OverlapSphere(tile.transform.position, 0.05f);

            foreach (Collider currentSurround in surroundingTiles)
            {
                tileInfo currentTileInfo = currentSurround.GetComponent<tileInfo>();
                if (currentTileInfo == null)// means gameobject is most likely not a tile
                {
                    continue;
                }
                if (currentSurround.transform.parent != colony)
                {
                    edgeTiles.Add(tile);
                    break;
                }
            }
        }
        return edgeTiles.ToArray();
    }

    public static GameObject getClosetPoint(GameObject target, GameObject thisColony)
    {
        int tileCount = thisColony.transform.childCount;
        GameObject canidateTile = null;
        float closestPosition = 999999f;

        for (int i = 0; i < tileCount; i++)
        {
            float currentDistance = Vector3.Distance(thisColony.transform.GetChild(i).position, target.transform.position);

            if (currentDistance < closestPosition)
            {
                canidateTile = thisColony.transform.GetChild(i).gameObject;
                closestPosition = currentDistance;
            }
        }

        if (canidateTile == null)
        {
            Debug.LogError("canidate position error! ");
        }
        return canidateTile;
    }

    public static GameObject getOwnerOfTile(GameObject target)
    {
        return target.transform.parent.gameObject;
    }

    public static GameObject nextClosestTile(GameObject ownedClosestTile, GameObject target, bool ignoreIfOwned = false)
    {
       
        Collider[] neighbouringTiles = Physics.OverlapSphere(ownedClosestTile.transform.position, 0.05f);
        int neighbourCount = neighbouringTiles.Length; // ITS ALWAYS THE SAME BUT YOU GOTTA DO YOUR DUE DILIGENCE
       
        float closestDistance = 999999f;
        GameObject candidateTitle = null;


        for (int i = 0; i < neighbourCount; i++)
        {
            
            GameObject currentTile = neighbouringTiles[i].gameObject;
            if (currentTile.layer != 6)// 6 is the moon layer
            {
                continue;
            }
            float currentDistance = Vector3.Distance(currentTile.transform.position, target.transform.position);
            

           
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                candidateTitle = currentTile;

            }


        }

        return candidateTitle;

    }
    static Dictionary<GameObject, GameObject> findPathingingMap(GameObject startTile, GameObject endTile)
    {
       
        Queue<GameObject> frontierTiles = new Queue<GameObject>();
        
        frontierTiles.Enqueue(startTile);
        
        Dictionary<GameObject, GameObject> cameFrom = new Dictionary<GameObject, GameObject>();
        cameFrom[startTile] = null;
         
        while (frontierTiles.Count > 0)
        {
            GameObject currentTile = frontierTiles.Dequeue();



            if (currentTile == endTile)
            {
                return cameFrom;
            }
          

            Collider[] neighbouringTiles = Physics.OverlapSphere(currentTile.transform.position, 0.05f);// 6 is the "theMoon" layer
         
            foreach (Collider currentTileNeighbourCollider in neighbouringTiles)
            {
                
                GameObject currentTileNeighbour = currentTileNeighbourCollider.gameObject;
                tileInfo currentTIleInfo = currentTileNeighbour.GetComponent<tileInfo>();
                if (currentTIleInfo == null)
                {
                    continue;
                }
                if (cameFrom.ContainsKey(currentTileNeighbour) || currentTileNeighbour == currentTile || currentTIleInfo.occupid == true)
                    {
                        continue;

                    }
                frontierTiles.Enqueue(currentTileNeighbour);
                cameFrom.Add(currentTileNeighbour, currentTile);
            }




        }
      
        return null; // means it couldnt find a path

    }
    public static List<GameObject> pathtingAlgorthim(GameObject startTile, GameObject endTile)
    {

        Dictionary<GameObject, GameObject> map = findPathingingMap(startTile, endTile);

        if (map == null)
        {
            return null;
        }
        List<GameObject> path = new List<GameObject>();
        GameObject currentTile = endTile;
        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = map[currentTile];


        }
        path.Reverse();

        return path;

    }
    /*

    public static GameObject bestNextTIle(GameObject colony, List<GameObject> outlineTiles) //determines the colonies best next tile to get
    {

        List<GameObject> ResourceTiles = new List<GameObject>();
        List<tileInfo> resourcesOfTiles = new List<tileInfo>();
        baseColonyAI thisColonyAI = colony.GetComponent<baseColonyAI>();





        for (int i = 0; i < outlineTiles.Count; i++)
        {

            Transform thisTileTransform = outlineTiles[i].GetComponent<Transform>();
            tileInfo thisTileInfo = outlineTiles[i].GetComponent<tileInfo>();
            GameObject thisTile = thisTileTransform.gameObject;

            ResourceTiles.Add(thisTile);
            resourcesOfTiles.Add(thisTileInfo);




        }

        resourcesOfTiles.Sort((a, b) => b.resource.CompareTo(a.resource));// makes so that tiles are sorted by resource value
        int bestTileIndex = 0;

        if (resourcesOfTiles.Count > 0)// this should always be the case but just to be sure
        {
            float[] valueOfTiles = new float[resourcesOfTiles.Count];
            for (int k = 0; k < resourcesOfTiles.Count; k++)
            {

                Vector3 positionOfTile = resourcesOfTiles[k].gameObject.transform.position;

                Collider[] surroundingTile = Physics.OverlapSphere(positionOfTile, 0.05f);

                for (int indexOfSurrounding = 0; indexOfSurrounding < surroundingTile.Length; indexOfSurrounding++)
                {
                    Debug.LogWarning(indexOfSurrounding + " " + surroundingTile.Length);
                    if (surroundingTile[indexOfSurrounding].transform.parent == colony)
                    {
                        valueOfTiles[k] += 1 - thisColonyAI.aggression; // aggressive colonies should be less caring of having good borders
                    }

                }

                valueOfTiles[k] += resourcesOfTiles[k].resource;







            }
            float bestValue = 0;


            for (int indexOfAllCanidates = 0; indexOfAllCanidates < valueOfTiles.Length; indexOfAllCanidates++)
            {
                Debug.LogError(valueOfTiles.Length);
                if (bestValue <= valueOfTiles[indexOfAllCanidates])
                {
                    bestValue = valueOfTiles[indexOfAllCanidates];
                    bestTileIndex = indexOfAllCanidates;
                    Debug.LogWarning(bestValue);
                }


            }



            Debug.Log(bestValue + " tinh;'ple:" + resourcesOfTiles[bestTileIndex].gameObject.name);




        }

        return resourcesOfTiles[bestTileIndex].gameObject;

    }
    */


    public static GameObject[] bestTilesurrouning(GameObject colony,int Length)
    {
         GameObject[] sortedGameObjects = new GameObject[Length];// end result

        baseColonyAI colonyAI = colony.GetComponent<baseColonyAI>();
        colonyScript colonyScript = colony.GetComponent<colonyScript>();
        GameObject[] outlineTiles = findEdgeTiles(colony);
        int numberOfOutlineTiles = outlineTiles.Length;

        if(Length > outlineTiles.Length)
        {
            Length =  outlineTiles.Length;
        }


        if (numberOfOutlineTiles <= 0)
        {
            Debug.LogError("not enough tiless to evalute");
            return null;
        }
        Dictionary<GameObject, float> dictonaryOfTiles = new Dictionary<GameObject, float>();



        for (int indexOfOutlineTiles = 0; indexOfOutlineTiles < numberOfOutlineTiles; indexOfOutlineTiles++)
        {
            float tileValue = 0;
            GameObject currentOutLineTile = outlineTiles[indexOfOutlineTiles];

            tileInfo currentTileInfo = currentOutLineTile.GetComponent<tileInfo>();

            tileValue += currentTileInfo.resource;

            Collider[] tilesSurroundingCurrent = Physics.OverlapSphere(currentOutLineTile.transform.position, 0.05f);


            for (int k = 0; k < tilesSurroundingCurrent.Length; k++)
            {


                if (tilesSurroundingCurrent[k].transform.parent.gameObject == colony.transform.gameObject)
                {

                    tileValue += (1 - colonyAI.aggression) * 2;

                }
            }
            Debug.Log(tileValue);

            if (dictonaryOfTiles.ContainsKey(currentOutLineTile) == false)
            {
                dictonaryOfTiles.Add(currentOutLineTile, tileValue);

            }
        }

            List<KeyValuePair<GameObject, float>> kvpOfTiles = dictonaryOfTiles.ToList();

            var sortedKvpOfTiles = kvpOfTiles.OrderByDescending(pair => pair.Value).ToList();

           

            for(int i = 0; i < Length ;i++)
            {
                sortedGameObjects[i] = sortedKvpOfTiles[i].Key;
            }

          

        
         return sortedGameObjects;


    }


    public static GameObject[] allColonyTiles(GameObject colony)
    {

        int amtOfChildrend = colony.transform.childCount;
        GameObject[] arrayTempTiles = new GameObject[amtOfChildrend];
        if (amtOfChildrend <= 0)
        {
            Debug.LogError("colony: " + colony + " attempted to get the size of their empire even though they have none");
            return null;
        }
        for (int p = 0; p < amtOfChildrend; p++)
        {
            arrayTempTiles[p] = colony.transform.GetChild(p).gameObject;


        }
        return arrayTempTiles;
    }


    public static bool purchasableAction(GameObject colony, BuildingStruct cost, GameObject tileOn, bool alsoBuy = false)
    {
        tileInfo tileOnInfo = tileOn.GetComponent<tileInfo>();
        Debug.Log(cost.moneyExpenses + " " + colony.gameObject.name + " " + tileOn.transform.parent);
        colonyScript thiscolonyScript = colony.GetComponent<colonyScript>();

        if (colony != tileOn.transform.parent.gameObject)
        {
            return false;
        }

        if (thiscolonyScript == null)
        {

            Debug.LogWarning("there was no colony script on the colony????");
            return false;
        }
        bool enoughResources = thiscolonyScript.resourcesOwned.resourceExpenses >= cost.resourceExpenses;
        bool enoughMOney = thiscolonyScript.resourcesOwned.moneyExpenses >= cost.moneyExpenses;
        bool enoughPeople = tileOnInfo.population >= cost.populationExpenses;

        bool allGood = enoughResources && enoughMOney && enoughPeople;



        if (allGood)
        {
            if (alsoBuy == false)
            {
                return true;
            }


            thiscolonyScript.resourcesOwned.resourceExpenses -= cost.resourceExpenses;
            thiscolonyScript.resourcesOwned.moneyExpenses -= cost.moneyExpenses;
            tileOnInfo.population -= cost.populationExpenses;
            return true;

        }
        else
        {
            return false;
            //BROKIE
        }
    }
    
    

 
    
    


    

    
    
     


}
