using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.Mathematics;

public class moonManipulator : MonoBehaviour
{
   
    int tilesCounted = 0;

    public float amountDiffrence;


    int childCount;

    float[,] closestDistance;


    GameObject[,] YCords = new GameObject[1, 1];

    GameObject[,] xCords = new GameObject[1, 1];


    void Start()
    {
        



        childCount = transform.childCount;
        



        for (int i = 0; i < childCount; i++)
        {
            gameObject.transform.GetChild(i).AddComponent<MeshCollider>();
            int currentXLength = YCords.GetLength(0);
            int currentYLength = YCords.GetLength(1);

            for (int i2 = 0; i2 < currentXLength; i2++)
            {
                if (YCords[i2, 0] != null)
                {
                    if (math.abs(YCords[i2, 0].transform.position.y - gameObject.transform.GetChild(i).transform.position.y) < amountDiffrence)
                    {
                        layerAdder(YCords, i2, transform.GetChild(i).gameObject);
                       


                    }
                    else
                    {
                        int oldLength = YCords.GetLength(0);
                    
                        YCords = newResizeArray(YCords, 1, oldLength + 1);
                        oldLength = YCords.GetLength(0);
               
                        
                        layerAdder(YCords, oldLength - 1 , transform.GetChild(i).gameObject);
                      

                    }
                }
                else
                {
                  
                    YCords[i2, 0] = transform.GetChild(i).gameObject;
                }
            }







      

            
            


        }


    }
    void layerAdder(GameObject[,] inputArray, int xPos, GameObject inputGameObject)
    {





        for (int verticalPosition = 0; verticalPosition < inputArray.GetLength(1); verticalPosition++)
        {


            if (inputArray.GetLength(1) < verticalPosition)
            {

                inputArray = newResizeArray(inputArray, verticalPosition + 1, inputArray.GetLength(0));
            }



            if (inputArray[xPos, verticalPosition] != null)
            {


                verticalPosition += 1;

            }
            else
            {


                inputArray[xPos, verticalPosition] = inputGameObject;
                tilesCounted += 1;
                Debug.Log("the game objects count amount to " + tilesCounted);
                return;
            }

        }
        Debug.Log("game shat itself and wrote at: " + xPos);
        
        

    }


    GameObject[,] newResizeArray(GameObject[,] initalArray, int vertical, int horizontal) // this function will !only! increase array size
    {

       
        
        GameObject[,] funtionTemp = new GameObject[vertical, horizontal];

        int maxX = Mathf.Min(initalArray.GetLength(0), funtionTemp.GetLength(0));
        int maxY = Mathf.Min(initalArray.GetLength(1), funtionTemp.GetLength(1));


        for (int i = 0; i < maxX; i += 1)
            {
                for (int i2 = 0; i2 < maxY; i2 += 1)
                {
                    funtionTemp[i, i2] = initalArray[i, i2];

                }

            }
        return funtionTemp;

            
    }


}
