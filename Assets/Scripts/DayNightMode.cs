using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightMode : MonoBehaviour
{
    public Light directionalLight;
    public Material[] skyboxes; 
    private int currentSkyboxIndex = 0;

    public float dayNightToggleInterval = 20.0f; 
    private float timer = 0.0f;

    void Start()
    {
        SetSkybox(currentSkyboxIndex);
    }

    void Update()
    {
        // Check for user input to toggle day/night manually
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleDayNight();
        }

        // Automatically toggle between day and night based on the specified interval
        timer += Time.deltaTime;
        if (timer >= dayNightToggleInterval)
        {
            ToggleDayNight();
            timer = 0.0f;
        }
    }

    void ToggleDayNight()
    {
        currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxes.Length;
        SetSkybox(currentSkyboxIndex);
    }

    void SetSkybox(int index)
    {
        RenderSettings.skybox = skyboxes[index];

       
        if (index == 0) 
        {
            RenderSettings.ambientIntensity = 1.0f;
            directionalLight.intensity = 1.0f;
        }
        else 
        {
            RenderSettings.ambientIntensity = 0.1f;
            directionalLight.intensity = 0.2f;
        }
    }
}
