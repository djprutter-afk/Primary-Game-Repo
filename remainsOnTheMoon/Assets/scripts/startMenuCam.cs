using UnityEngine;
using System;

public class startMenuCam : MonoBehaviour
{
   
    [SerializeField] float maxRotationSpeed = 10;
    [SerializeField] float timeMultiplier = 0.01f;
    // Update is called once per frame
    Vector3 rotationNormalized = new Vector3(1,0,0);
    float timeSinceStart = 0;
    void Update()
    {
        timeSinceStart += Time.deltaTime;

        float yChange =0.5f- Mathf.PerlinNoise1D(timeSinceStart *timeMultiplier)*0.1f;
        float xChange = 0.5f-Mathf.PerlinNoise1D(timeSinceStart +500 *timeMultiplier)*0.1f;
        rotationNormalized += new Vector3(0,xChange,yChange);
        
        rotationNormalized.Normalize();
        rotationNormalized.x = 1;
        


        transform.Rotate(rotationNormalized * maxRotationSpeed * Time.deltaTime);
     
    }


}
