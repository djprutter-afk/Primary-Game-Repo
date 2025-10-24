
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;



public class cameraRotate : MonoBehaviour
{

    public float maxZoomOut;
    public float minZoomOut;
    [SerializeField] float moveSpeed;

    public float timeWanted;

    int timeAmount;

    [SerializeField]float timeElapsed;
    Vector2 mouseLockPosition;
    Vector2 mouseMove;

    public GameObject playerCamera;

    public GameObject playerCameraTarget;

    [Header("screen shake setup")]
    public bool screenIsShaking;
    Vector3 screenShakePos;
    [SerializeField] float screenShakeMaxTime = 1;
    [SerializeField] float maxMovment = 0;
    [SerializeField] float movementSlope = 5f;
    [SerializeField] float totalScreenShakeCurrentTIme;
    [SerializeField] float CurrentScreenShakeTime;
    [SerializeField] float CurrentScreenShakeTimeMax;
    bool targetGoingOutofBounds = false;

    void Start()
    {
        expansionScript.explosion += explosionHandler;
    }
    void explosionHandler(GameObject explosion)
    {
        expansionScript currentExplosion = explosion.GetComponent<expansionScript>();
        screenShakeSetup(currentExplosion.Power / 20, 5);

    }


    void Update()
    {


        if (Input.GetMouseButtonDown(1))
        {
            
            mouseLockPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {

            Mouse.current.WarpCursorPosition(mouseLockPosition);

            Cursor.lockState = CursorLockMode.Confined;

            float vericalMouse = Input.GetAxis("Mouse Y");
            float horiztonalMouse = Input.GetAxis("Mouse X");

            mouseMove = new Vector2(horiztonalMouse, vericalMouse);

        }
        else
        {
            mouseMove = Vector2.zero;

            Cursor.lockState = CursorLockMode.None;
        }

        transform.eulerAngles += new Vector3(0, mouseMove.x * moveSpeed, -mouseMove.y * moveSpeed);


        if (Input.mouseScrollDelta != Vector2.zero)
        {






            float absoluteDistance = Vector3.Distance(playerCameraTarget.transform.position, Vector3.zero);// uses 0,0 as refrence

            float mouseScrollMovement = Input.mouseScrollDelta.x - Input.mouseScrollDelta.y;

            if ((absoluteDistance >= minZoomOut) && (absoluteDistance <= maxZoomOut))
            {


                playerCameraTarget.transform.localPosition += new Vector3(mouseScrollMovement, 0, 0);

                float targetPosition = Vector3.Distance(playerCameraTarget.transform.position, Vector3.zero);

                if (targetPosition > minZoomOut && targetPosition < maxZoomOut) // if target is within bounds
                {
                    targetGoingOutofBounds = false;

                    timeElapsed = 0;

                }
                else
                {
                    if (targetGoingOutofBounds == false)
                    {
                        timeElapsed = 0;
                        targetGoingOutofBounds = true;
                    }

                }
             


            }
            absoluteDistance = Vector3.Distance(playerCameraTarget.transform.position, Vector3.zero);

            if (absoluteDistance < minZoomOut)
            {

                float changeAmount = minZoomOut - absoluteDistance;
              

                playerCameraTarget.transform.localPosition += new Vector3(changeAmount, 0, 0);


            }
            if (absoluteDistance > maxZoomOut)
            {

                float changeAmount = maxZoomOut - absoluteDistance;
               
                playerCameraTarget.transform.localPosition += new Vector3(changeAmount, 0, 0);


            }













        }

        if (playerCamera.transform.position != playerCameraTarget.transform.position)
        {
            timeElapsed += Time.deltaTime;



            float interpolationRatio = Mathf.Clamp01(timeElapsed / timeWanted);

            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, playerCameraTarget.transform.position, interpolationRatio);

            //Debug.Log(interpolationRatio + " " + timeElapsed + " " + timeWanted);

        }

        if (timeElapsed > timeWanted && false)
        {

            playerCamera.transform.position = playerCameraTarget.transform.position;
        }















        if (screenIsShaking == true)
        {
            totalScreenShakeCurrentTIme += Time.deltaTime;
            CurrentScreenShakeTime += Time.deltaTime;

        }

        if (totalScreenShakeCurrentTIme >= screenShakeMaxTime)
        {
            screenIsShaking = false;
            
        }
        if (CurrentScreenShakeTime >= CurrentScreenShakeTimeMax && screenIsShaking == true)
        {
            maxMovment /= 1+(1.03f*Time.deltaTime);




            screenShakeReArc(maxMovment);
        }
        

        if (maxMovment <= 0.002)
            {
                maxMovment = 0;
                screenIsShaking = false;
            }
        










    }
    public void screenShakeSetup(float Power, float totalTime)
    {
        movementSlope = 0.05f;
        screenIsShaking = true;
        Debug.Log(Power + " " + totalTime);
        maxMovment += Power;
        totalScreenShakeCurrentTIme = 0;

        screenShakeMaxTime = totalTime;
        screenShakeReArc(maxMovment);
        
        


    }
    void screenShakeReArc(float MovementAmt)
    {
        movementSlope = 0.05f;
        movementSlope += UnityEngine.Random.Range(-movementSlope / 3, movementSlope / 3);
        CurrentScreenShakeTimeMax = movementSlope;
        CurrentScreenShakeTime = 0;
        

        float xPosition = UnityEngine.Random.Range(-MovementAmt, MovementAmt);
        float yPosition = UnityEngine.Random.Range(-MovementAmt, MovementAmt);
        float zPosition = UnityEngine.Random.Range(-MovementAmt, MovementAmt);

        screenShakePos = new Vector3(xPosition, yPosition, zPosition);
        playerCamera.transform.position += screenShakePos;
        
    }
    
    


}
