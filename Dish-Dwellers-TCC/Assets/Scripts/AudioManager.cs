using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioSource musicaAtual;

    private float defaultMasterVolume = 0.5f;
    public float currentMasterVolume;

    private string masterVolumeKey = "Master Volume";
    public SliderController masterVolumeSlider;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = defaultMasterVolume;
        masterVolumeSlider.sliderObject.onValueChanged.AddListener(MudaVolume);
        LoadVolume();
    }

    public void MudaVolume(float value)
    {
        audioSource.volume = value;
        SaveVolume(masterVolumeKey, value);
    }

    public void SaveVolume(string key, float value){
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
    public void LoadVolume(){
        currentMasterVolume = PlayerPrefs.GetFloat(masterVolumeKey);
        MudaVolume(currentMasterVolume);
        masterVolumeSlider.MudarValueSlider(currentMasterVolume);
    }
}
