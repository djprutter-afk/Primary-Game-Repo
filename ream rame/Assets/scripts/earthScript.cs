using UnityEngine;

public class earthScript : MonoBehaviour
{
    [SerializeField] float TurnSpeed;
    [SerializeField] float rotateAroundSpeed;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.down * Time.deltaTime * TurnSpeed);
        transform.RotateAround(Vector3.zero, Vector3.up, rotateAroundSpeed * Time.deltaTime);
        
        
    }
}
