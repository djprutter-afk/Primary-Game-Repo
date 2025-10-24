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


    public float slope;
    public float endDiameter;
    [SerializeField] GameObject tilePosition;
    MeshRenderer thisMesh;
[SerializeField] Material genericMaterial;
    float time; // seconds

    Material explosionMaterial;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        localGenericMaterial = Instantiate(genericMaterial);





        thisMesh = GetComponent<MeshRenderer>();

        //transform.position = tilePosition.transform.position;

        Material explosionMaterialshared = thisMesh.sharedMaterial;
        explosionMaterial = Instantiate(explosionMaterialshared);
        thisMesh.material = explosionMaterial;


        thisMesh.material.SetFloat("_fadeout", 1);
        explosion!.Invoke(gameObject);




    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;








        float expansionDiameter = (math.sqrt(time) / slope) * endDiameter;
        float completenes = 1 - expansionDiameter / endDiameter;

        thisMesh.material.SetFloat("_fadeout", completenes);
        transform.localScale = new Vector3(expansionDiameter, expansionDiameter, expansionDiameter);

        if (transform.localScale.x >= endDiameter)
        {
            Destroy(gameObject);
        }








    }

    void OnDestroy()
    {
        Destroy(thisMesh.material);
        Destroy(localGenericMaterial);
    }
    void OnTriggerEnter(Collider thiscollider)
    {
        
    
    
        
        
        tileInfo thisTileInfo = thiscollider.gameObject.GetComponent<tileInfo>();

        if (thisTileInfo != null)
        {
            
            float tileprotect = 1 - thisTileInfo.tileProtection;
            thisTileInfo.population /= (int)(Power * tileprotect) + 1;
            thisTileInfo.development /= Power * tileprotect / 2 + 1;

            tileVisuals thistileVis = thiscollider.gameObject.GetComponent<tileVisuals>();
            Material material = thistileVis.tileMaterial;
            Color newColour = Color.Lerp(material.color, Color.black, Power / 4);

            localGenericMaterial.color = newColour;


            thistileVis.setupTileVisuals(localGenericMaterial);
            

        }
        
    }










}
