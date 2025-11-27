using UnityEngine;

public class interiorMoon : MonoBehaviour
{
    [SerializeField] float becomeCratorChance = 0.005f;
    [SerializeField] Material genericMaterial;
    [SerializeField] Material moonMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(Transform visTile in gameObject.GetComponentsInChildren<Transform>())
        {
            if(visTile == transform)
            {
                continue;
            }

            visTile.gameObject.AddComponent<MeshCollider>();
           



           
            tileVisuals skbidid = visTile.gameObject.AddComponent<tileVisuals>();
            
            skbidid.setupTileVisuals(moonMaterial);
            
        }
         foreach(Transform visTile in gameObject.GetComponentsInChildren<Transform>())
        {
            if(visTile == transform)
            {
                continue;
            }
            float chance = UnityEngine.Random.value * 10;
            if(becomeCratorChance > chance)
            {
              
                createNewCrator(visTile);
            }
        }

            
    }

    void createNewCrator(Transform CenterOfCrator)
    {
        Debug.Log("created new crator at " + CenterOfCrator);
        float sizeOfcrator = UnityEngine.Random.Range(0.05f, 0.26f);
        float Power = UnityEngine.Random.Range(0.09f, 0.2f);
        Collider[] surroundingTiles = Physics.OverlapSphere(CenterOfCrator.position, sizeOfcrator);
        foreach(Collider currentTile in surroundingTiles)
        {
            if(currentTile.transform.parent != transform)
            {
                continue;
            }
            Material localGenericMaterial = Instantiate(genericMaterial);

            tileVisuals thistileVis = currentTile.gameObject.GetComponent<tileVisuals>();
            Material material = thistileVis.tileMaterial;
            Color newColour = Color.Lerp(material.color, Color.black, Power);

            localGenericMaterial.color = newColour;


            thistileVis.setupTileVisuals(localGenericMaterial);
            Destroy(localGenericMaterial);
        }

    }

    
}
