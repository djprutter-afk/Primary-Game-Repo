using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AIMultithreader : MonoBehaviour
{
    public List<baseColonyAI> artificialIntelligences = new List<baseColonyAI>();
    int expectedAmtOfAIs;
    struct AIColonyInfo
    {
       [ReadOnly] public  baseColonyAI theAI;

        public Dictionary<string, agentBelief> beliefs;
        public HashSet<agentAction> actions;
        public HashSet<AgentGoal> goals;
    }
    

    NativeArray<AIColonyInfo> AIColonies;

    void reEvaluateAIs()
    {
        AIColonies = new NativeArray<AIColonyInfo>(artificialIntelligences.Count,Allocator.Persistent);

        for(int i = 0; i < artificialIntelligences.Count; i++)
        {
            AIColonies[i] = new AIColonyInfo
            {
                theAI = artificialIntelligences[i],
                beliefs = artificialIntelligences[i].beliefs,
                actions = artificialIntelligences[i].actions,
                goals = artificialIntelligences[i].goals
                               
            };
        }
        expectedAmtOfAIs = artificialIntelligences.Count;
    }

    struct goapPlanner : IJobFor
    {
        [ReadOnly] AIColonyInfo[] infos;
        public void Execute(int AIToWorkOn)
        {
            AIColonyInfo aIColonyInfo = infos[AIToWorkOn];
            
        }
    }

   void updateAis()
    {
        
         reEvaluateAIs();
        
        
    }
}
