using UnityEngine;

public class RadialLayoutUI : MonoBehaviour {
    [Range(0f, 360f)]
    public float anguloEmGraus = 360f, offsetAngulo = 0f;
    public float raio = 1f;
    public float corteAngulo => anguloEmGraus / (transform.childCount > 0 ? transform.childCount : 1);

    void OnValidate() {
        SetarLayout();
    }

    public void AdicionarFilho(Transform filho) {
        if (filho == null) return;

        filho.SetParent(transform, false);
        SetarLayout();
    }

    public void RemoverFilho(Transform filho) {
        if (filho == null) return;

        filho.SetParent(null, false);
        SetarLayout();
    }

    [ContextMenu("Setar Layout")]
    public void SetarLayout() {
        int numeroDeFilhos = transform.childCount;
        if (numeroDeFilhos == 0) return;

        Vector2[] posicoes = CalcularRadial(numeroDeFilhos);

        for (int i = 0; i < numeroDeFilhos; i++) {
            Transform filho = transform.GetChild(i);
            if (filho == null) continue;

            RectTransform rectTransform = filho.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = posicoes[i];
        }
    }

    public Vector2[] CalcularRadial(int numeroDeFilhos = 0) {
        if (numeroDeFilhos <= 0) return new Vector2[0];

        float anguloAtual = offsetAngulo * Mathf.Deg2Rad;
        if (numeroDeFilhos == 1) return new Vector2[1] {new Vector2(Mathf.Cos(anguloAtual), Mathf.Sin(anguloAtual)) * raio };

        int numeroDeCortes = anguloEmGraus == 360 ? numeroDeFilhos : numeroDeFilhos - 1;
        float anguloEmRadianos = (anguloEmGraus * Mathf.Deg2Rad) / numeroDeCortes;

        Vector2[] posicoes = new Vector2[numeroDeFilhos];

        for (int i = 0; i < numeroDeFilhos; i++) {
            posicoes[i] = GetPosFromAngle(anguloAtual);
            anguloAtual += anguloEmRadianos;
        }

        return posicoes;
    }

    public Vector2 GetPosFromAngle(float anguloEmRadianos) {
        return new Vector2(Mathf.Cos(anguloEmRadianos), Mathf.Sin(anguloEmRadianos)) * raio;
    }

    public Vector3 FromVector2(Vector2 vetor) {
        return new Vector3(vetor.x, vetor.y, 0f);
    }

    public GameObject GetChild(int index) {
        if (index < 0 || index >= transform.childCount) return null;
        return transform.GetChild(index).gameObject;
    }

    public GameObject GetChild(Vector2 direction) {
        if (transform.childCount == 0) return null;
        if (transform.childCount == 1) return transform.GetChild(0).gameObject;

        float angulo = Mathf.Atan2(direction.y, direction.x);
        Vector3 posicao = FromVector2(GetPosFromAngle(angulo));

        GameObject filhoMaisProximo = null;
        float distanciaMinima = float.MaxValue;
        foreach (Transform filho in transform) {
            if (filho == null) continue;

            Vector3 posicaoFilho = filho.position;
            float distancia = Vector3.Distance(posicao, posicaoFilho);
            if (distancia < distanciaMinima) {
                distanciaMinima = distancia;
                filhoMaisProximo = filho.gameObject;
            }
        }

        return filhoMaisProximo;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;

        int numeroDeFilhos = transform.childCount;
        Transform filhoAnterior = null;

        foreach (Transform filho in transform) {
            if (filho == null) continue;

            Gizmos.DrawLine(transform.position,  filho.position);
            Gizmos.DrawSphere(filho.position, 0.1f);

            if (filhoAnterior != null) {
                Gizmos.DrawLine(filho.position, filhoAnterior.position);
            }

            filhoAnterior = filho;
        }
    }

}
