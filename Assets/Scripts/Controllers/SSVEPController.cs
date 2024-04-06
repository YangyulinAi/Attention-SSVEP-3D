/**
 * Author: Yangyulin Ai
 * Email: Yangyulin-1@student.uts.edu.au
 * Date: 2024-03-18
 */

using UnityEngine;
using UnityEngine.UI;

public class SSVEPController : MonoBehaviour
{
    //Parameters, setting in Main.cs
    private float frequency = 10f; // Frequency in Hz (Hertz)
    private float switchInterval;

    private GameObject textObject;

    private Image image; // Image component of the panel
    private float timeCounter = 0f;

    private KeyCode targetKey; // Target key to press



    void Start()
    {
        image = GetComponent<Image>(); // Get the Image component
        switchInterval = 1f / (2f * frequency); // Calculate the interval for changing colors
    }

    void Update()
    {
        image.enabled = true;
        timeCounter += Time.deltaTime;

        // Determine if it's time to switch colors
        if (timeCounter >= switchInterval)
        {
            // Reset the timer
            timeCounter = 0f;

            // Switch colors
            if (image.color == Color.white)
            {
                image.color = Color.black;
            }
            else
            {
                image.color = Color.white;
            }
        }
    }

    //Getter and Setter
    public void SetFrequency(float frequency)
    {
        this.frequency = frequency; // Provides a public method to set the frequency
        switchInterval = 1f / (2f * this.frequency); // Recalculate switchInterval
    }

    public void SetTextObject(GameObject textObject)
    {
        this.textObject = textObject;
    }

}
