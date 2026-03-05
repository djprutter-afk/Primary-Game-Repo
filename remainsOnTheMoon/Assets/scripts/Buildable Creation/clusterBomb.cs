
using System.Collections.Generic;
using System.Linq;
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
            List<Collider> colliders = Physics.OverlapSphere(randomSpot,1).ToList();
            List<Vector3> tilePositionsHit = colliders.Where(x=>x.GetComponent<tileInfo>() != null).Select(x=>x.transform.position).ToList();
            Vector3[] tilePositionsHitSorted = tilePositionsHit.OrderBy(x=>Vector3.Distance(x,randomSpot)).ToArray();
            if(tilePositionsHitSorted.Length <= 0)
            {
                Debug.LogWarning("couldnt find any positions to explode!");
                return;
            }
            GameObject explosion = Instantiate(explosionObject,tilePositionsHitSorted.First(), transform.rotation);
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
