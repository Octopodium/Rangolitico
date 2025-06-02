using UnityEngine;
using System.Collections;

public class TransicaoDeTela : MonoBehaviour {
    RectTransform rect;
    [SerializeField] float duracaoDaTransicao = 1.0f;
    [SerializeField] Vector2 resolution = new Vector2(1920, 1080);

    private void OnEnable() {
        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(-resolution.x, 0.0f);
        StartCoroutine(TransicaoHorizontal(-resolution.x, 0.0f, false));
    }

    public void TerminarTransicao() {
        StartCoroutine(TransicaoHorizontal(0, -resolution.x, true));
    }

    IEnumerator TransicaoHorizontal(float valO, float valF, bool desativar) {
        float timer = 0.0f;
        float interpolator;

        while (timer < duracaoDaTransicao) {
            timer += Time.fixedDeltaTime;

            float step = timer / duracaoDaTransicao;

            interpolator = Mathf.Lerp(valO, valF, step * step);
            rect.sizeDelta = new Vector2(interpolator, 1);

            yield return new WaitForFixedUpdate();
        }

        rect.sizeDelta = resolution;
        
        if (desativar) {
            gameObject.SetActive(false);
        }
    }
}
