using UnityEngine;

public class genericBuildingScript : MonoBehaviour
{
    /// <summary>
    /// destroies the gameObject if not need after start, such as when its just used to add a building to a tile.
    /// </summary>
    [SerializeField] bool destroyOnCreate = true;// destruction should be the norm, not always though 
    void Start()
    {
        if(destroyOnCreate == true)
        {
            Destroy(gameObject);
        }
    }

 
}
