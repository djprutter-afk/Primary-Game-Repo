using UnityEngine;

public class allBuildables : MonoBehaviour
{
    [System.Serializable]
    public class buildableGameObject//building
    {

        public float moneyExpenses;
        public float resourceExpenses;
        public int populationExpenses;
        public GameObject buildableObject; // be sure to assign position
        public string nameOfBuildable;
        public bool isBuilding;

    }
    [System.Serializable]
    public class buildingCatagory//thing that contains buildings
    {
        public string catagoryName;
        public Color catagoryColour;
        public buildableGameObject[] arrayOfBuildings;// building contained within the catagory

    }



    [System.Serializable]
    public class missileCatagory
    {
        public string catagoryName = "Missiles";
        public Color catagoryColour;
        public buildableGameObject smallMissile;
    }





    [System.Serializable]
    public class allCatagories
    {
        public missileCatagory missileCatagory;

    }


    public allCatagories allOfThecats;
    
    
}
