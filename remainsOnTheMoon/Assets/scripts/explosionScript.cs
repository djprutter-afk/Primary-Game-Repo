using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;

public class expansionScript : MonoBehaviour
{
    public static event System.Action<GameObject> explosion;
    public event System.Action explosionEnd;
   
    /// <summary>
    /// ranges from 0 to 1, 0 being nothing meanwhile 1 being total destruction of the tiles it touches
    /// </summary>
    public float Power;
    
    Material localGenericMaterial;


    public float timeToFinishSeconds;
    public float endDiameter;
    [SerializeField] GameObject tilePosition;
    [SerializeField] GameObject fire;
    [SerializeField] GameObject explosionLight;

    MeshRenderer thisMesh;
[SerializeField] Material genericMaterial;
    float time; // seconds

    Material explosionMaterial;

Light lightComp;

float imsinae;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightComp = explosionLight.GetComponent<Light>();
        imsinae =1+ Power/2;
        lightComp.intensity =imsinae;
        localGenericMaterial = Instantiate(genericMaterial);





        thisMesh = GetComponent<MeshRenderer>();

        //transform.position = tilePosition.transform.position;

        Material explosionMaterialshared = thisMesh.sharedMaterial;
        explosionMaterial = Instantiate(explosionMaterialshared);
        thisMesh.material = explosionMaterial;


        thisMesh.material.SetFloat("_fadeout", 1);
        explosion.Invoke(gameObject);




    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;





        if (transform.localScale.x >= endDiameter)
        {
            explosionEnd.Invoke();
            Destroy(gameObject);
        }


        
        float expansionDiameter = math.sqrt(time /timeToFinishSeconds) * endDiameter;
        
        float completenes = 1 - expansionDiameter / endDiameter;
        float completenessSqrt=(math.sqrt(completenes))*(imsinae);
lightComp.intensity =completenessSqrt;

thisMesh.material.SetFloat("_fadeout",completenes) ;
thisMesh.material.SetFloat("_intensity",completenes * 10);
        transform.localScale = new Vector3(expansionDiameter, expansionDiameter, expansionDiameter);









    }

    void OnDestroy()
    {
        Destroy(thisMesh.material);
        Destroy(localGenericMaterial);
    }
    void OnTriggerEnter(Collider thiscollider)
    {

        
    
    
        
        
        tileInfo thisTileInfo = thiscollider.gameObject.GetComponent<tileInfo>();
        tileVisuals thistileVis = thiscollider.gameObject.GetComponent<tileVisuals>();
        if (thisTileInfo != null)
        {
            GameObject fireObject = Instantiate(fire);
            fireScript theFireScript = fireObject.GetComponent<fireScript>();
            theFireScript.setToTile(thisTileInfo.gameObject,1f,this);
          
            thisTileInfo.population /= (int)(Power) + 1;
            thisTileInfo.development /= Power ;
            //thiscollider.gameObject

            
            //tileInfo thistileInfo = thiscollider.gameObject.GetComponent<tileInfo>();

               

           
            

        }
        else if(thistileVis != null)
        {
             Material material = thistileVis.tileMaterial;
            Color newColour = Color.Lerp(material.color, Color.black, Power / 2);
            localGenericMaterial.color = newColour;


            thistileVis.setupTileVisuals(localGenericMaterial);

        }
        
    }










}
