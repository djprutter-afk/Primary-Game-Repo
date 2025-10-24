using UnityEngine;

public class chatScript : MonoBehaviour// not implempted yet but player can interact with ai by saying things, like swearing and including an AI's name will make it made
{

    [SerializeField] GameObject chatItem;

    void Start()
    {
        colonyScript.newChatMsg += NewChat;
        
    }

    void NewChat(string username, string messge)
    {

        GameObject newMsg = Instantiate(chatItem, transform);

        

        
    }

    
    void Update()
    {
        
    }
}
