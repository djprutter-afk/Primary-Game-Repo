using Unity.Mathematics;
using UnityEngine;

public class tileVisuals : MonoBehaviour
{
    public bool isSelected = false;
    float damageAmount; 
    public Material tileMaterial;//thesess should only change based on ownership
    [SerializeField]Color tileColour;//thesess should only change based on ownership

    [SerializeField] Material changeMaterial;
    [SerializeField]Color changedColour;


    bool isReady = false; // if everything is setup


    float fadeDuration; // duration of fade effect

    float currentDuration; //
    MeshRenderer tileRender;

   

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

    public void damageChange(float changeAmt)
    {

    }




    public void setupTileVisuals(Material inputMaterial)
    {
        isReady = false;

        tileRender = GetComponent<MeshRenderer>();

        tileMaterial = Instantiate(inputMaterial);

    



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
        if ((currentDuration > 0) && isReady)
        {
            currentDuration -= Time.deltaTime;

            float percentage =0.01f- Mathf.Clamp01((currentDuration - 0.5f) / fadeDuration);

            tileRender.material.SetFloat("_alpha", percentage);
        }
        
    }




}
