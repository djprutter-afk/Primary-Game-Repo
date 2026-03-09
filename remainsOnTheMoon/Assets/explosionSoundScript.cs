using UnityEngine;

public class explosionSoundScript : MonoBehaviour
{
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        Invoke(nameof(destroySelf), audioSource.clip.length);
    }

    void destroySelf()
    {
        Destroy(gameObject);
    }

    
}
