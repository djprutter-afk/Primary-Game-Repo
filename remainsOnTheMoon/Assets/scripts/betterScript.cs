using Unity.VisualScripting;
using UnityEngine;

public class moonScript : MonoBehaviour
{
    [Header("resource amount should be at least 100")]
    [SerializeField] float resourceAmount;

    
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
            transformOfChild.AddComponent<tileVisuals>();
            TileInformation.ResourceCapacity = Random.Range(resourceAmount *0.5f, resourceAmount);
        }

    }
  
    
}

    // Update is called once per frame
   
