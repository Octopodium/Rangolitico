using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioSource musicaAtual;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.03f;
    }

    //Previsão de Slider Para o futuro...
    public void MusicSoundController(float value)
    {
        audioSource.volume = value;
    }
}
