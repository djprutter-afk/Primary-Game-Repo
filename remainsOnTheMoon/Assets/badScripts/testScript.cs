using UnityEngine;

public class testScript : MonoBehaviour
{
    public bool enable;
    public Camera theCamera;

    public Material changeMaterial;

    Camera cameraComponent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(enable);
        cameraComponent = theCamera.GetComponent<Camera>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Ray cameraRay = cameraComponent.ScreenPointToRay(Input.mousePosition);

   

        if (Physics.Raycast(cameraRay, out RaycastHit hitThing))
        {
            

            Renderer rend = hitThing.transform.GetComponent<Renderer>();
            rend.material = changeMaterial;
        }

        
        

       
        
    }
}
