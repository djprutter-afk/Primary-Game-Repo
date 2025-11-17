using Unity.VisualScripting;
using UnityEngine;

public class moonScript : MonoBehaviour
{// if youre reading this then say test in the commit
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
            TileInformation.resource = Random.Range(0.0f, resourceAmount);
        }

    }
  
    
}

    // Update is called once per frame
   
