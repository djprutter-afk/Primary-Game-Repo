

using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;

public class gameSetup : MonoBehaviour
{
    public float minDistance;
    public GameObject theMoon;
    public int colonyAmount;

    [Header("All posssible colony types")]
    public GameObject[] colonyTypes;






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] colonyPositions = new GameObject[colonyAmount];

        for (int i = 0; i < colonyAmount; i++)
        {

            int indexOfColonies = Random.Range(0, colonyTypes.Length);
            GameObject createdColony = Instantiate(colonyTypes[indexOfColonies], Vector3.zero, transform.rotation);
            colonyStart colonyStartScript = createdColony.GetComponent<colonyStart>();

            int indexOfAllColonyPositions = Random.Range(0, theMoon.transform.childCount);
            int trys = 0;



            for (int i2 = 0; i2 < colonyPositions.Length; i2++)
            {
                if (trys >= 200)
                {
                    Debug.LogWarning("failed to find valid position for " + createdColony.name);
                    return;


                }
                else
                {
                    trys++;
                }

                if (colonyPositions[i2] == null)
                {
                    Debug.LogWarning(i2);
                    colonyPositions[i2] = theMoon.transform.GetChild(indexOfAllColonyPositions).gameObject;
                    break;

                }


                if (Vector3.Distance(theMoon.transform.GetChild(indexOfAllColonyPositions).position, colonyPositions[i2].transform.position) < minDistance)
                {
                    Debug.LogWarning("had to do a redo, distance was:" + Vector3.Distance(theMoon.transform.GetChild(indexOfAllColonyPositions).position, colonyPositions[i2].transform.position));

                    indexOfAllColonyPositions = Random.Range(0, theMoon.transform.childCount);
                    i2 = 0;

                }






            }
            Debug.Log(colonyPositions.Length);
            for (int indexOfThing = 0; indexOfThing < colonyPositions.Length; indexOfThing++)
            {
                if (colonyPositions[indexOfThing] == null)
                {
                    Debug.Log("found supervalid position at: " + indexOfThing);
                    colonyPositions[indexOfThing] = theMoon.transform.GetChild(indexOfAllColonyPositions).gameObject;
                    break;

                }
            }

            colonyStartScript.colonyStartPosition = theMoon.transform.GetChild(indexOfAllColonyPositions).gameObject;
            Debug.Log("found valid position for " + createdColony.name + " at:" + colonyStartScript.colonyStartPosition);


        }



    }

    void colonyPlacer(GameObject colony)
    {
        for (int tileIndex = 0; tileIndex < colonyAmount; tileIndex++)
        {

        }

    }

    

    bool distanceChecker(GameObject colony, GameObject[] listOfAllColonies)
    {
        for (int colonyIndex = 0; colonyIndex < colonyAmount; colonyIndex++)
        {
            if (Vector3.Distance(listOfAllColonies[colonyIndex].transform.position, colony.transform.position) < minDistance)
            {
                return false;
            }

        }
        return false;

    }

 
}
