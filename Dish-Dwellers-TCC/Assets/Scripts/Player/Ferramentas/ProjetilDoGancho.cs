using UnityEngine;

public class ProjetilDoGancho : MonoBehaviour {
    Gancho gancho;
    LineRenderer lineRenderer;
    Transform baseGancho;

    float velocidade;
    Vector3 direcao;
    bool movendo = true;

    public Transform ganchoHolder, conexaoGancho;

    public void Inicializar(Gancho gancho, Vector3 direcao, float velocidade) {
        this.gancho = gancho;
        this.direcao = direcao;
        this.velocidade = velocidade;
        this.lineRenderer = gancho.lineRenderer;
        this.baseGancho = gancho.ganchoSpawn;

        ganchoHolder.forward = direcao;

        PintarCorda(0);
        lineRenderer.SetPosition(0, baseGancho.position);
        lineRenderer.SetPosition(1, conexaoGancho.position);
        lineRenderer.enabled = true;
    }

    void FixedUpdate() {
        if (movendo) transform.position += direcao * velocidade * Time.fixedDeltaTime;

        lineRenderer.SetPosition(0, baseGancho.position);
        lineRenderer.SetPosition(1, conexaoGancho.position);

        float distancia = Vector3.Distance(baseGancho.position, conexaoGancho.position);
        PintarCorda(distancia);

        if (distancia > gancho.distanciaMaxima || PassouPorCortantes()){
            gancho.DestruirGancho();
        }
    }

    public void PintarCorda(float distancia) {
        float porcentagem = (distancia >= 0) ? distancia / gancho.distanciaMaxima : 0;
        Color cor = gancho.gradienteCorda.Evaluate(porcentagem);

        lineRenderer.startColor = cor;
        lineRenderer.endColor = cor;
    }

    /// <summary>
    /// Retorna verdadeiro caso a corda tenha colidido com algum objeto cortante
    /// </summary>
    public bool PassouPorCortantes(float distancia = -1f) {
        if (distancia == -1f) distancia = Vector3.Distance(baseGancho.position, conexaoGancho.position);

        RaycastHit hit;
        if (Physics.Raycast(baseGancho.position, conexaoGancho.position - baseGancho.position, out hit, distancia, gancho.layerCortante)) {
            return true;
        }

        return false;
    }

    void OnTriggerEnter(Collider collider) {
        if (!movendo) return;

        Ganchavel ganchavel = collider.gameObject.GetComponent<Ganchavel>();
        if (ganchavel != null && ganchavel.PodeSerGanchado()) {
            movendo = false;
            gancho.SetarGanchado(ganchavel);
        } else {
            gancho.DestruirGancho();
        }
    }

}
