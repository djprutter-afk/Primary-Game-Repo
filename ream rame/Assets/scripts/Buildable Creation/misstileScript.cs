
using System.Drawing;

using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class misstileScript : MonoBehaviour
{
    public float missileSpeed;
    public float explosionpower = 1;


    public bool launchDebug;
    public GameObject targetDebug;
    buildableScript thisUnitScript;
    public float forceAmt;

    Rigidbody thisrigidBody;
    Vector3 targetPosition;
    Vector3 startPosition;
    float timeToObject = 1f;
    float currentTime = 0f;

    bool isFlying;
    [Header("missile setup")]
    [SerializeField] float missileHeight;
    Vector3 orginalPosition;
    float missileLaunchSpeed = 0f;

    bool launching = false;

    [Header("explosion setup")]
    [SerializeField] GameObject explosionObject;
    [SerializeField] float power;
    [SerializeField] float slope;
    [SerializeField] float endDiameter;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisrigidBody = GetComponent<Rigidbody>();
        thisUnitScript = GetComponent<buildableScript>();
        buildableScript thisBuildableScript = GetComponent<buildableScript>();

        thisBuildableScript.launch += targetAndLaunch;
        thisBuildableScript.Move += moveMissile;










    }

    void Update()
    {
        if (launchDebug == true)
        {

            targetAndLaunch(targetDebug);

        }

        if (launching)
        {
            missileLaunchSpeed += Time.deltaTime / 4;
            transform.position += transform.forward * Time.deltaTime * missileLaunchSpeed;
            float distanceFromOrignalPos = Vector3.Distance(orginalPosition, transform.position);

            if (distanceFromOrignalPos >= missileHeight)
            {
                startPosition = transform.position;
                isFlying = true;
                launching = false;
            }
        }

        if (isFlying == true && targetPosition != null)
        {
            currentTime += Time.deltaTime;
            float distanceTotarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceTotarget <= 0.01)
            {
                explode();


            }

            float percentageDone = currentTime / timeToObject;


            Vector3 slepPos = Vector3.Slerp(startPosition, targetPosition, percentageDone);
            Vector3 slepPosAhead = Vector3.Slerp(startPosition, targetPosition, percentageDone + 0.02f);
            transform.position = slepPos;

            Transform pointAtPosition = transform;



            pointAtPosition.LookAt(slepPosAhead);






        }
    }



    public void targetAndLaunch(GameObject target)
    {

        // just think here, missile should go up then slowly turn towards target, problem is if target is on the other side of the planet, maybe use a pathfind algorithim?
        // maybe the furtherer the missile is to the target maybe the slower it turns? and the closer it goes faster
        currentTime = 0f;
        // i have found slerp

        tileInfo tileonInfo = thisUnitScript.tileOn.GetComponent<tileInfo>();
        tileonInfo.occupid = false;




        Debug.Log("misssile launch");
        targetPosition = target.transform.position;

        float distanceTotarget = Vector3.Distance(transform.position, targetPosition);
        timeToObject = distanceTotarget * 10 / missileSpeed;

        orginalPosition = transform.position;









        launching = true;






    }

    void explode()
    {
        GameObject explosion = Instantiate(explosionObject, transform.position, transform.rotation);

        expansionScript skbidi = explosion.GetComponent<expansionScript>();

        skbidi.Power = power;
        skbidi.slope = slope;
        skbidi.endDiameter = endDiameter;
        thisUnitScript.FinsihedAction();
        Destroy(gameObject);

    }
    void moveMissile(GameObject target)
    {
        buildableScript thisBuildableScript = GetComponent<buildableScript>();
        thisBuildableScript.moveToTileSetup(target);
        
    }





}
