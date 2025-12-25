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
            GameObject explosion = Instantiate(explosionObject,randomSpot, transform.rotation);
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
