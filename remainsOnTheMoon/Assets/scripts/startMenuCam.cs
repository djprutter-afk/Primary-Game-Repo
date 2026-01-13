using UnityEngine;
using System;

public class startMenuCam : MonoBehaviour
{
    public float speed;
    public float maxRotationSpeed;
     float xRotation;
 float yRotation=0.5f;
 float zRotation;

    // Update is called once per frame
    void Update()
    {
       xRotation += UnityEngine.Random.Range(-1f,1f) * Time.deltaTime *speed;
       yRotation += UnityEngine.Random.Range(-1f,1f) * Time.deltaTime * speed;
       zRotation += UnityEngine.Random.Range(-1f,1f) * Time.deltaTime * speed;
        
        xRotation = System.Math.Clamp(xRotation,-maxRotationSpeed,maxRotationSpeed);
        yRotation = System.Math.Clamp(yRotation,-maxRotationSpeed,maxRotationSpeed);
        zRotation = System.Math.Clamp(zRotation,-maxRotationSpeed,maxRotationSpeed);
        transform.Rotate(xRotation,yRotation,zRotation);
    }
}
