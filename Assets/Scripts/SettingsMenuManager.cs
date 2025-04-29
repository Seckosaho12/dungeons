using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuManager : MonoBehaviour
{
   public TMP_Dropdown graphicsDropdown;
   public TMP_Dropdown resolutionDropdown;
   public Slider masterVolumeSlider;
   public Slider sfxVolumeSlider;
   public Slider musicVolumeSlider;

   void Start()
   {
      // Set up graphics quality dropdown
      graphicsDropdown.ClearOptions();
      List<string> options = new List<string>();
      for (int i = 0; i < QualitySettings.names.Length; i++)
      {
         options.Add(QualitySettings.names[i]);
      }
      graphicsDropdown.AddOptions(options);

      // Load saved graphics quality
      int savedGraphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
      graphicsDropdown.value = savedGraphicsQuality;
      QualitySettings.SetQualityLevel(savedGraphicsQuality);
      graphicsDropdown.RefreshShownValue();

      // Set up resolution dropdown
      resolutionDropdown.ClearOptions();
      List<string> resolutions = new List<string>();
      foreach (Resolution res in Screen.resolutions)
      {
         resolutions.Add(res.width + " x " + res.height);
      }
      resolutionDropdown.AddOptions(resolutions);

      // Load saved resolution
      int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", System.Array.IndexOf(Screen.resolutions, Screen.currentResolution));
      resolutionDropdown.value = savedResolutionIndex;
      Resolution savedResolution = Screen.resolutions[savedResolutionIndex];
      Screen.SetResolution(savedResolution.width, savedResolution.height, Screen.fullScreen);
      resolutionDropdown.RefreshShownValue();

      // Assign functions to relevant UI elements
      graphicsDropdown.onValueChanged.AddListener(delegate { ChangeGraphicsQuality(); });
      resolutionDropdown.onValueChanged.AddListener(delegate { ChangeResolution(); });
      masterVolumeSlider.onValueChanged.AddListener(delegate { ChangeMasterVolume(); });
      sfxVolumeSlider.onValueChanged.AddListener(delegate { ChangeSFXVolume(); });
      musicVolumeSlider.onValueChanged.AddListener(delegate { ChangeMusicVolume(); });

      // Load saved volume settings
      masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
      sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
      musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
   }

   public void ChangeGraphicsQuality()
   {
      int qualityLevel = graphicsDropdown.value;
      QualitySettings.SetQualityLevel(qualityLevel);
      PlayerPrefs.SetInt("GraphicsQuality", qualityLevel);
   }

   public void ChangeResolution()
   {
      Resolution[] resolutions = Screen.resolutions;
      int selectedResolutionIndex = resolutionDropdown.value;
      Resolution selectedResolution = resolutions[selectedResolutionIndex];
      Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
      PlayerPrefs.SetInt("ResolutionIndex", selectedResolutionIndex);
   }

   public void ChangeMasterVolume()
   {
      float volume = masterVolumeSlider.value;
      AudioManager.instance.SetMasterVolume(volume);
      PlayerPrefs.SetFloat("MasterVolume", volume);
   }

   public void ChangeSFXVolume()
   {
      float volume = sfxVolumeSlider.value;
      AudioManager.instance.SetSFXVolume(volume);
      PlayerPrefs.SetFloat("SFXVolume", volume);
   }

   public void ChangeMusicVolume()
   {
      float volume = musicVolumeSlider.value;
      AudioManager.instance.SetMusicVolume(volume);
      PlayerPrefs.SetFloat("MusicVolume", volume);
   }
}
