using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource button_click;

    public AudioSource food_destroy_sound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClick()
    {
        button_click.Play();
    }

    public void OnFoodDestroy()
    {
        food_destroy_sound.Play();
    }
}
