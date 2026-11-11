using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class OptionMenu : MonoBehaviour
{   
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float bufferTime;

    private float masterVolume;
    private float sfxVolume;
    private float musicVolume;
    private float backgroundVolume;

    private Coroutine saveBuffer;



    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider miscSlider;

    private TMP_Dropdown fullscreenDropdown;
    private Toggle pixelToggle;

    void Awake()
    {
        FixNulls();

        LoadOptions();
    }

    private void FixNulls()
    {
        if (audioMixer == null) audioMixer = transform.parent.Find("MainMenu").GetComponent<AudioMixer>();

        foreach (Slider slider in transform.parent.GetComponentsInChildren<Slider>(true))
        {
            switch (slider.gameObject.name)
            {
                case "Master Volume Slider":
                    if (masterSlider == null) masterSlider = slider;
                    break;
                case "SFX Volume Slider":
                    if (sfxSlider == null) sfxSlider = slider;
                    break;
                case "Music Volume Slider":
                    if (musicSlider == null) musicSlider = slider;
                    break;
                case "Misc Volume Slider":
                    if (miscSlider == null) miscSlider = slider;
                    break;
            }
        }

        foreach (TMP_Dropdown dropdown in transform.parent.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            switch (dropdown.gameObject.name)
            {
                case "Fullscreen Dropdown":
                if (fullscreenDropdown == null) fullscreenDropdown = dropdown;
                    break;
            }
        }
    
        foreach (Toggle toggle in transform.parent.GetComponentsInChildren<Toggle>(true))
        {
            switch (toggle.gameObject.name)
            {
                case "Pixel Toggle":
                    if (pixelToggle == null) pixelToggle = toggle;
                    break;
            }
        }
    }

    public void MasterVolumeSlider(float value)
    {
        masterVolume = value;
        audioMixer.SetFloat("Master Volume", value);
        TryToSave();
    }

    public void SFXVolumeSlider(float value)
    {
        sfxVolume = value;
        audioMixer.SetFloat("SFX Volume", value);
        TryToSave();
    }

    public void MusicVolumeSlider(float value)
    {
        musicVolume = value;
        audioMixer.SetFloat("Music Volume", value);
        TryToSave();
    }

    public void BackgroundVolumeSlider(float value)
    {
        backgroundVolume = value;
        audioMixer.SetFloat("Misc Volume", value);
        TryToSave();
    }

    public void FullscreenDropdown(int index)
    {
        Screen.fullScreenMode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.MaximizedWindow,
            3 => FullScreenMode.Windowed,
            _ => Screen.fullScreenMode
        };

        TryToSave();
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        TryToSave();
    }

    public void SetPixel(bool isPixel)
    {
        ConfigManager.Instance.IsDither = isPixel;
    }

    private void TryToSave()
    {
        if (saveBuffer == null)
            saveBuffer = StartCoroutine(SaveAfterBuffer());
    }

    private IEnumerator SaveAfterBuffer()
    {
        yield return new WaitForSeconds(bufferTime);

        SaveOptions();

        saveBuffer = null;
    }

    private void SaveOptions()
    {
        SaveSystem.SaveOptions(new SaveSystem.OptionsSaveData
        {
            MasterVolume = masterVolume,
            SFXVolume = sfxVolume,
            MusicVolume = musicVolume,
            BackgroundVolume = backgroundVolume,
            Quality = QualitySettings.GetQualityLevel(),
            FullscreenMode = fullscreenDropdown.value,
            IsPixel = pixelToggle.isOn
        });
    }

    private void LoadOptions()
    {
        var options = SaveSystem.LoadOptions();

        if (options == null)
            return;

        masterVolume = options.MasterVolume;
        sfxVolume = options.SFXVolume;
        musicVolume = options.MusicVolume;
        backgroundVolume = options.BackgroundVolume;

        masterSlider.value = options.MasterVolume;
        sfxSlider.value = options.SFXVolume;
        musicSlider.value = options.MusicVolume;
        miscSlider.value = options.BackgroundVolume;

        fullscreenDropdown.value = options.FullscreenMode;
        FullscreenDropdown(options.FullscreenMode);

        audioMixer.SetFloat("Master Volume", options.MasterVolume);
        audioMixer.SetFloat("SFX Volume", options.SFXVolume);
        audioMixer.SetFloat("Music Volume", options.MusicVolume);
        audioMixer.SetFloat("Misc Volume", options.BackgroundVolume);

        QualitySettings.SetQualityLevel(options.Quality);
        pixelToggle.isOn = options.IsPixel;
        SetPixel(options.IsPixel);
    }

    public void ApplyOptions(SaveSystem.OptionsSaveData options)
    {
        masterVolume = options.MasterVolume;
        sfxVolume = options.SFXVolume;
        musicVolume = options.MusicVolume;
        backgroundVolume = options.BackgroundVolume;

        masterSlider.value = options.MasterVolume;
        sfxSlider.value = options.SFXVolume;
        musicSlider.value = options.MusicVolume;
        miscSlider.value = options.BackgroundVolume;

        fullscreenDropdown.value = options.FullscreenMode;
        FullscreenDropdown(options.FullscreenMode);

        audioMixer.SetFloat("Master Volume", options.MasterVolume);
        audioMixer.SetFloat("SFX Volume", options.SFXVolume);
        audioMixer.SetFloat("Music Volume", options.MusicVolume);
        audioMixer.SetFloat("Misc Volume", options.BackgroundVolume);

        QualitySettings.SetQualityLevel(options.Quality);
        pixelToggle.isOn = options.IsPixel;
        SetPixel(options.IsPixel);
    }
}