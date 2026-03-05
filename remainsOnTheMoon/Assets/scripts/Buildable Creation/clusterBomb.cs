
using UnityEngine;

public class clusterBomb : misstileScript
{
    [SerializeField] int explosiveAmt;
    [SerializeField] float maxDistanceRadius;
    public override void explode() 
    {
        for(int i = 0; i <explosiveAmt;i++)
        {
             Vector3 randomSpot = transform.position;
             
      
            randomSpot.x += UnityEngine.Random.Range(-maxDistanceRadius, maxDistanceRadius);
            randomSpot.y += UnityEngine.Random.Range(-maxDistanceRadius, maxDistanceRadius);
            randomSpot.z += UnityEngine.Random.Range(-maxDistanceRadius, maxDistanceRadius);
                 // FIX THIS, DOES NOT WORK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Vector3 directionVector =  randomSpot;
        
        Vector3 normalizedDirection = directionVector.normalized;
            Ray ray = new Ray(randomSpot,normalizedDirection);
            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(ray,out hitInfo);

            GameObject explosion = Instantiate(explosionObject,hitInfo.point, transform.rotation);
            expansionScript skbidi = explosion.GetComponent<expansionScript>();
            skbidi.Power = power * Random.Range(0.5f,1f);
            skbidi.timeToFinishSeconds = timeToFinsishInSeconds* Random.Range(0.7f,1.3f);
            skbidi.endDiameter = endDiameter* Random.Range(0.8f,1.3f);

        }
       // Application.OpenURL("https://www.youtube.com/watch?v=KKJprZqU_oU");
        thisUnitScript.FinsihedAction();
        Destroy(gameObject);
    }
}
