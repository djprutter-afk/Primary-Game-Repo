using UnityEngine;

public class sensor : MonoBehaviour
{
    baseColonyAI myAI;
    colonyScript myColonyScript;
    public TriValueStruct resourcesOwned;
    public TriValueStruct resourceChange;
    
    public GameObject[] OwnedTiles;
    public GameObject[] OwnedBuildables;
    /// <summary>
    /// used to determine how an ai feel about another colony
    /// </summary>
    struct AIRelationshipsInfo
    {
        GameObject colony; // the colony this info is refering to
        public float hatred; // the degree an ai will hate this paticular colony, values over 0.50 will make the ai start considering conflict. 0 to 1
        public float fear; // fear for the indvidual colony. 0 to 1
    }
    AIRelationshipsInfo[] AIFeelings;

    void Start()
    {
        myAI = GetComponent<baseColonyAI>();
        //myAI.AITick += sensorUpdate;
    }
    void sensorUpdate()
    {
        resourcesOwned = myColonyScript.resourcesOwned;
        resourceChange = myColonyScript.totalIncome();
        OwnedTiles = myColonyScript.allTilesOwned.ToArray();
        OwnedBuildables = myColonyScript.ownedBuildables.ToArray();

    }
}
