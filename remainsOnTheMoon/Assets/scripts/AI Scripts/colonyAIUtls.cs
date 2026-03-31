
using UnityEngine;
public class  AIUtils
{
    baseColonyAI colonyAI;
    colonyScript thisColonyScript;
    public AIUtils(baseColonyAI baseColonyAI)
    {
        colonyAI = baseColonyAI;
        thisColonyScript = baseColonyAI.thisColonyScript;
        
    }
        Vector3 GetAveragePosition()
        {
            Vector3 averagePosition = Vector3.zero;
            foreach(GameObject tile in thisColonyScript.allTilesOwned)
            {
                averagePosition += tile.transform.position;
            }
            return averagePosition /= thisColonyScript.allTilesOwned.Count;
        }   
    /// <summary>
    /// returns total fear and updates the buildable valued
    /// </summary>
    /// <returns></returns>
    /// 
    public float updateFear()
    {
           
        

        Vector3 colonyCenter = GetAveragePosition();
         float totalMilitaryOfSelf = 0f;
            foreach(GameObject indivdualBuildable in thisColonyScript.ownedBuildables)
            {
                 buildableScript indivdualBuildableScript = indivdualBuildable.GetComponent<buildableScript>();
                foreach (buildableScript.AIBuildableInfo.biInfoStuct infoStuct in indivdualBuildableScript.purposes)
                {
                   if (infoStuct.purpose == buildableScript.AIBuildableInfo.buildablePurposes.offensive)
                   {
                       float distanceToCenter = Vector3.Distance(colonyCenter,indivdualBuildable.transform.position);// entire moon is 2 units across
                       
                       totalMilitaryOfSelf += infoStuct.strength * (2-distanceToCenter);
                      

                   }

               }
            }
        float totalFear = 0f;
        foreach(var colonyInfo in colonyAI.otherColonyInfos)
        {
            colonyInfo.evaluateThreatLevel(thisColonyScript,totalMilitaryOfSelf);
            totalFear+= colonyInfo.threatLevel;
        }
        totalFear /= colonyAI.otherColonyInfos.Count;


        // expansion infos


         colonyAI.valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.offensive] = totalFear *0.5f;

        colonyAI.valueOfBuildables[buildableScript.AIBuildableInfo.buildablePurposes.suicidieOffensive] = totalFear *0.40f;

        return totalFear;
      

    }
}