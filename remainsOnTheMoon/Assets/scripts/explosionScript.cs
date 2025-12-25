using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;

public class expansionScript : MonoBehaviour
{
    public static event System.Action<GameObject> explosion;
   
    /// <summary>
    /// ranges from 0 to 1, 0 being nothing meanwhile 1 being total destruction of the tiles it touches
    /// </summary>
    public float Power;
    
    Material localGenericMaterial;


    public float timeToFinishSeconds;
    public float endDiameter;
    [SerializeField] GameObject tilePosition;
    [SerializeField] GameObject explosionLight;

    MeshRenderer thisMesh;
[SerializeField] Material genericMaterial;
    float time; // seconds

    Material explosionMaterial;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Light lightComp = explosionLight.GetComponent<Light>();
        lightComp.intensity = Power;
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
            Destroy(gameObject);
        }


        
        float expansionDiameter = math.sqrt(time /timeToFinishSeconds) * endDiameter;
      
        float completenes = 1 - expansionDiameter / endDiameter;


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
        if (thisTileInfo != null)// this is so incredibly sloppy but idc
        {
            
          
            thisTileInfo.population /= (int)(Power) + 1;
            thisTileInfo.development /= Power ;

            
            tileInfo thistileInfo = thiscollider.gameObject.GetComponent<tileInfo>();

               

           
            

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
