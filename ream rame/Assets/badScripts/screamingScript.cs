using UnityEngine;

public class screamingScript : MonoBehaviour
{
    void Start()
    {
        Invoke("scream", 3);
    }

    void scream()
    {
        Collider[] hitcolliderss = Physics.OverlapSphere(transform.position, 0.05f);
        foreach (var hitcollider in hitcolliderss)
        {
           
            
        }
    }





}
