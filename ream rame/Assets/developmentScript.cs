using UnityEngine;

public class developmentScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildableScript myBuildableScript = GetComponent<buildableScript>();
        tileInfo tileonInfo = myBuildableScript.tileOn.GetComponent<tileInfo>();
        tileonInfo.development += 1;
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
// small test