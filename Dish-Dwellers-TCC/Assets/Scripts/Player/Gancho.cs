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


    bool acabouDePuxar = false;

    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    public void Acionar() {
        if (ganchado != null) {
            PuxarGancho();
            acabouDePuxar = true;
        } else if (gancho == null) {
            this.jogador.MostrarDirecional(true);
        } else {
            DestruirGancho();
        }
    }

    public void Soltar() {
        this.jogador.MostrarDirecional(false);
        
        if (gancho == null && ganchado == null && !acabouDePuxar) {
            AtirarGancho();
        }

        acabouDePuxar = false;
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

    /// <summary>
    /// Atira o gancho na direção que o jogador está olhando
    /// </summary>
    public void AtirarGancho() {
        gancho = Instantiate(ganchoPrefab, ganchoSpawn.position, Quaternion.identity);
        ProjetilDoGancho projetil = gancho.GetComponent<ProjetilDoGancho>();
        projetil.Inicializar(this, jogador.direcao, velocidadeGancho);
    }

    /// <summary>
    /// Destroi o gancho
    /// </summary>
    public void DestruirGancho() {
        Destroy(gancho);
        UpdateCorda(null);
    }

    /// <summary>
    /// Gancha um objeto ganhavel
    /// </summary>
    /// <param name="ganchavel">Objeto ganchavel</param>
    public void SetarGanchado(Ganchavel ganchavel) {
        ganchado = ganchavel;
        UpdateCorda(ganchavel.meio);
    }

    /// <summary>
    /// Puxa o gancho
    /// </summary>
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

    /// <summary>
    /// Retorna verdadeiro caso a corda tenha colidido com algum objeto cortante
    /// </summary>
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

    /// <summary>
    /// Atualiza o visual da corda e verifica se passou por objetos cortantes
    /// </summary>
    /// <param name="posicaoTarget">Posição do gancho ou objeto ganchado (ou null se não houver) </param>
    /// <param name="distancia">Distância entre a origem do gancho e a posição final</param>
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

}
