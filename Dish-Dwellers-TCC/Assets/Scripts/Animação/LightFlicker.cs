using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LightFlicker : MonoBehaviour {
    public Color emissiveColor = Color.yellow;       // Cor da luz da vela
    public float minIntensity = 0.2f;                // Brilho m�nimo
    public float maxIntensity = 2.0f;                // Brilho m�ximo
    public float flickerSpeed = 1.0f;                // Velocidade da varia��o

    private Material _material;
    private float _targetIntensity;
    private float _currentIntensity;
    private float _timer;

    void Start() {
        _material = GetComponent<Renderer>().material;
        _currentIntensity = Random.Range(minIntensity, maxIntensity);
        _targetIntensity = Random.Range(minIntensity, maxIntensity);
        _timer = 0f;
    }

    void Update() {
        _timer += Time.deltaTime * flickerSpeed;

        if (_timer >= 1f) {
            _timer = 0f;
            _targetIntensity = Random.Range(minIntensity, maxIntensity);
        }

        // Suaviza a transi��o entre intensidades
        _currentIntensity = Mathf.Lerp(_currentIntensity, _targetIntensity, Time.deltaTime * flickerSpeed);

        // Aplica a cor emissiva no material
        Color finalColor = emissiveColor * _currentIntensity;
        _material.SetColor("_EmissionColor", finalColor);

        // Atualiza ilumina��o global para refletir emiss�o
        DynamicGI.SetEmissive(GetComponent<Renderer>(), finalColor);
    }
}
