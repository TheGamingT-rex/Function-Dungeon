using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSliderUpdate : MonoBehaviour
{
    [SerializeField] private TMP_InputField audioVolumeText;
    [SerializeField] private Slider audioSlider;
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void UpdateSlider()
    {
        // Try to parse the whole input field text as an integer.
        // If parsing fails, fall back to the slider's current value.
        if (int.TryParse(audioVolumeText.text, out int intValue))
        {
            // Convert to float for the slider, clamp to slider range.
            float clamped = Mathf.Clamp(intValue, (int)audioSlider.minValue, (int)audioSlider.maxValue);
            audioSlider.value = clamped;

            // Keep the input text consistent with the clamped value.
            audioVolumeText.text = ((int)clamped).ToString();
        }
        else
        {
            // Invalid input: reset input to the slider's current (integer) value.
            audioVolumeText.text = Mathf.RoundToInt(audioSlider.value).ToString();
        }
        
        audioManager.ChangeVolume((int)audioSlider.value);
    }
    
    public void UpdateText()
    {
        // Update the text field to match the slider's value.
        audioVolumeText.text = Mathf.RoundToInt(audioSlider.value).ToString();
        audioManager.ChangeVolume((int)audioSlider.value);
    }
}
