using UnityEngine;

public class Gancho : MonoBehaviour, Ferramenta {
    public LayerMask layerGanchavel, layerNaoGanchavel;
    public LineRenderer lineRenderer;
    public Transform ganchoSpawn;
    public GameObject ganchoPrefab;
    public float distanciaMaxima = 10f;
    public float velocidadeGancho = 20f;

    public Color corPadrao;
    public Gradient gradienteCorda;

    GameObject gancho;
    Ganchavel ganchado;
    Player jogador;

    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    public void Acionar() {
        if (ganchado != null) {
            ganchado.onDesganchado.Invoke();
            ganchado = null;
            UpdateCorda(null);
        } else if (gancho == null) {
            gancho = Instantiate(ganchoPrefab, ganchoSpawn.position, Quaternion.identity);
            ProjetilDoGancho projetil = gancho.GetComponent<ProjetilDoGancho>();
            projetil.Inicializar(this, jogador.direcao, velocidadeGancho);
        } else {
            Destroy(gancho);
            UpdateCorda(null);
        }
    }

    public void Soltar() {
        
    }

    public void DestruirGancho() {
        Destroy(gancho);
        UpdateCorda(null);
    }

    public void SetarGanchado(Ganchavel ganchavel) {
        ganchado = ganchavel;
        UpdateCorda(ganchavel.transform);
    }

    public void UpdateCorda(Transform target, float distancia = -1) {
        if (target == null) {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, ganchoSpawn.position);
        lineRenderer.SetPosition(1, target.position);

        if (distancia >= 0) {
            float porcentagem = distancia / distanciaMaxima;
            Color cor = gradienteCorda.Evaluate(porcentagem);

            lineRenderer.startColor = cor;
            lineRenderer.endColor = cor;
        } else {
            lineRenderer.startColor = corPadrao;
            lineRenderer.endColor = corPadrao;
        }
    }

    void FixedUpdate() {
        if (gancho != null) {
            float distancia = Vector3.Distance(ganchoSpawn.position, gancho.transform.position);
            UpdateCorda(gancho.transform);

            if (distancia > distanciaMaxima) {
                DestruirGancho();
            }
        } else if (ganchado != null) {
            float distancia = Vector3.Distance(ganchoSpawn.position, ganchado.transform.position);
            UpdateCorda(ganchado.transform, distancia);

            if (distancia > distanciaMaxima) {
                ganchado.onDesganchado.Invoke();
                ganchado = null;
                UpdateCorda(null);
            }
        }
    }

}
