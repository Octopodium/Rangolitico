using UnityEngine;
using System.Collections;

public class SpritePisca : MonoBehaviour {
    [SerializeField] private Material piscaMat;
    Material defaultMat;
    Renderer renderer;

    [Tooltip("Para resultados melhores, recomenda-se o uso de numeros impares")]
    public int piscadas;


    private void Awake() {
        renderer = GetComponent<Renderer>();
        defaultMat = renderer.material;
    }

    public void Piscar(float duracao) {
        StartCoroutine(PiscaSprite(duracao));
    }

    IEnumerator PiscaSprite(float duracao) {
        float step = duracao / piscadas;

        for (int i = 0; i < piscadas; i++) {
            if (i % 2 == 0) {
                renderer.material = piscaMat;
            }
            else {
                renderer.material = defaultMat;
            }
            float timer = step;

            while (timer > 0) {
                timer -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

        renderer.material = defaultMat;
    }
}
