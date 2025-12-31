using Unity.Mathematics;
using UnityEngine;

public class tileVisuals : MonoBehaviour
{
    

public float intialTHING = 0.01F;
    public bool isSelected = false;

    public Material tileMaterial;//thesess should only change based on ownership
    [SerializeField] Material changeMaterial;
    [SerializeField]Color changedColour;


    bool isReady = false; // if everything is setup


   [SerializeField] float fadeDuration; // duration of fade effect

    [SerializeField]float currentDuration; //
    MeshRenderer tileRender;
    void OnEnable()
    {
        
    }



    void OnDestroy()
    {
        Destroy(tileMaterial);
        
    }
    public void Select(Material selectMaterial)
    {
        tileRender = GetComponent<MeshRenderer>();
        tileRender.material = selectMaterial;
        isReady = false;
        isSelected = true;

    }
    public void deSelect()
    {
        tileRender = GetComponent<MeshRenderer>();
        tileRender.material = tileMaterial;
        isReady = true;
        isSelected = false;


    }
    
    




    public void setupTileVisuals(Material inputMaterial,float alhpa = 0.01f)
    {
        if(tileMaterial != null)
        {
            Destroy(tileMaterial);
        }
        isReady = false;

        tileRender = GetComponent<MeshRenderer>();
        
        tileMaterial = Instantiate(inputMaterial);
        intialTHING = alhpa;
        




        if (isSelected == false)
        {
            tileRender.material = tileMaterial;
        }
        

        isReady = true; // means tile is ready for gaming
    }

    public void fadeEffect(float duration, Material material)
    {
        
        fadeDuration = duration;

        currentDuration = duration;

        changeMaterial = material;

        changedColour = material.color;


    }

    void Update()
    {
        if ((currentDuration >= 0) && isReady)
        {
            currentDuration -= Time.deltaTime;

            float percentage =  Mathf.Clamp01(currentDuration  / fadeDuration) +0.01f;

            tileRender.material.SetFloat("_alpha", percentage +intialTHING);
        }
        
    }




}
