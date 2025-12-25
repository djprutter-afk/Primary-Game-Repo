using Unity.VisualScripting;
using UnityEngine;

public class MoonScript : MonoBehaviour
{
    [SerializeField] GameObject visualMoon;
    [SerializeField] float resourceAmountMax;
     [SerializeField] float resourceAmountMin;
     [SerializeField] float transparancy;
    public Material moonMaterial;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {


        int childCount = transform.childCount;




        for (int i = 0; i < childCount; i++)
        {
            Transform transformOfChild = gameObject.transform.GetChild(i);

            transformOfChild.AddComponent<MeshCollider>();
            //tileVisualReactiveness visualEffects = transformOfChild.AddComponent<tileVisualReactiveness>();




            tileInfo TileInformation = transformOfChild.AddComponent<tileInfo>();
            tileVisuals skbidid = transformOfChild.AddComponent<tileVisuals>();
            //TileInformation.visualMoon = visualMoon;
            skbidid.setupTileVisuals(moonMaterial,0.01f);
         
            TileInformation.resource = Random.Range(resourceAmountMin, resourceAmountMax);
        }

    }
  
    
}

    // Update is called once per frame
   
