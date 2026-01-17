using UnityEngine;
using UnityEngine.VFX;

public class fireScript : MonoBehaviour
{   float timeTolastFor = 0;
bool isActive = false;
bool isChillingOut = false;
VisualEffect visualEffect;
   
    public void setToTile(GameObject tile,float power,expansionScript Explosion)
    {
        visualEffect = GetComponent<VisualEffect>();
        visualEffect.pause = true;
        timeTolastFor = 30f * power;

        transform.position = tile.transform.position;
        transform.LookAt(Vector3.zero);
        transform.Rotate(Vector3.left *90);
        Explosion.explosionEnd += startFire;

        transform.localScale = new Vector3(0.03f * power, 0.03f * power, 0.03f * power);


    }
    void Update()
    {
        if(isChillingOut== true && visualEffect.aliveParticleCount <=0)
        {
            Destroy(gameObject);
        }
        if(isActive == false)
        {
            return;
        }
        timeTolastFor -= Time.deltaTime;
        visualEffect.SetInt("spawnAmt",(int)timeTolastFor *2);
        if(timeTolastFor<=0 && isChillingOut == false)
        {
            visualEffect.Stop();
            isChillingOut =true;
          
        }
    }

    void startFire()
    {
        visualEffect.pause = false; 
        isActive= true;
    }
}
