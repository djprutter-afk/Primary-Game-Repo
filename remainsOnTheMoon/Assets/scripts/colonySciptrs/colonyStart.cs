using System.Collections.Generic;
using UnityEngine;

public class colonyStart : MonoBehaviour // every tile that is a child of the colony is a tile owned by that colony

{
    // looking back at this script makes me realise this script has no right to exist and should be merged in the colonyScript script : TODO
    public GameObject colonyStartPosition;
    public Material colonyMaterial;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (colonyStartPosition != null)
        {
            colonyStartPosition.transform.SetParent(transform);

            tileVisuals startTileVisualss = colonyStartPosition.GetComponent<tileVisuals>();

            if (startTileVisualss != null)
            {
                startTileVisualss.setupTileVisuals(colonyMaterial,0.5f);
            }
            
            
            

            //tileVisualReactiveness tileEffects = colonyStartPosition.GetComponent<tileVisualReactiveness>();





                /*
                    Renderer rend = colonyStartPosition.transform.GetComponent<Renderer>();
                    rend.material = colonyColour;
                */
        }
        else
        {

            Debug.Log("bruh");
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
