using UnityEngine;

public class Gancho : MonoBehaviour, Ferramenta {
    public LineRenderer lineRenderer;
    public Transform ganchoSpawn;
    public GameObject ganchoPrefab;

    [Header("Configurações do Gancho")]
    public LayerMask layerGanchavel, layerCortante;
    public float distanciaMaxima = 10f;
    public float velocidadeGancho = 20f;
    
    public Color corPadrao;
    public Gradient gradienteCorda;

    [Header("Configurações de Puxada")]
    public float forcaDePuxada = 1f;
    public float alturaDePuxada = 6.5f;

    GameObject gancho;
    Ganchavel ganchado;
    Player jogador;

    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    public void Acionar() {
        if (ganchado != null) {
            PuxarGancho();
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
        UpdateCorda(ganchavel.meio);
    }

    public void PuxarGancho() {
        if (ganchado == null) return;

        ganchado.HandlePuxado();
        UpdateCorda(null);

        Rigidbody rb = ganchado.GetComponent<Rigidbody>();

        if (rb != null) {
            float distancia = Vector3.Distance(ganchoSpawn.position, ganchado.meio);

            Vector3 direcao = (ganchoSpawn.position - ganchado.meio).normalized;
            Vector3 arremeco = Vector3.up * alturaDePuxada;

            rb.AddForce((direcao * distancia * forcaDePuxada) + arremeco, ForceMode.Impulse);
        }
        
        ganchado = null;
    }

    public bool PassouPorCortantes() {
        Transform target = null;
        if (ganchado != null) target = ganchado.transform;
        else if (gancho != null) target = gancho.transform;
        else return false;

        RaycastHit hit;
        if (Physics.Raycast(ganchoSpawn.position, target.position - ganchoSpawn.position, out hit, distanciaMaxima, layerCortante)) {
            return true;
        }

        return false;
    }

    public void UpdateCorda(Vector3? posicaoTarget, float distancia = -1) {
        if (posicaoTarget == null) {
            lineRenderer.enabled = false;
            return;
        }

        if (PassouPorCortantes()) {
            if (gancho != null) DestruirGancho();
            else if (ganchado != null) {
                ganchado.HandleDesganchado();
                ganchado = null;
            }
            
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, ganchoSpawn.position);
        lineRenderer.SetPosition(1, (Vector3) posicaoTarget);

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
            UpdateCorda(gancho.transform.position);

            if (distancia > distanciaMaxima) {
                DestruirGancho();
            }
        } else if (ganchado != null) {
            float distancia = Vector3.Distance(ganchoSpawn.position, ganchado.meio);
            UpdateCorda(ganchado.meio, distancia);

            if (distancia > distanciaMaxima) {
                ganchado.HandleDesganchado();
                ganchado = null;
                UpdateCorda(null);
            }
        }
    }

}
