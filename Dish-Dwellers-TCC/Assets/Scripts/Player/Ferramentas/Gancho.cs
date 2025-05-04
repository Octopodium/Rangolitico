using UnityEngine;

public class Gancho : MonoBehaviour, Ferramenta {
    public LineRenderer lineRenderer;
    public Transform ganchoSpawn;
    public GameObject ganchoPrefab;

    [Header("Configurações do Gancho")]
    public LayerMask layerCortante;
    public float distanciaMaxima = 10f;
    public float velocidadeGancho = 20f;
    
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

    /// <summary>
    /// Atira o gancho na direção que o jogador está olhando
    /// </summary>
    public void AtirarGancho() {
        gancho = Instantiate(ganchoPrefab, ganchoSpawn.position, Quaternion.identity);
        ProjetilDoGancho projetil = gancho.GetComponent<ProjetilDoGancho>();
        projetil.Inicializar(this, jogador.direcao, velocidadeGancho);
    }

    /// <summary>
    /// Destroi o gancho (as vezes chamado pelo próprio projetil de gancho)
    /// </summary>
    public void DestruirGancho() {
        Destroy(gancho);
        lineRenderer.enabled = false;

        if (ganchado != null) {
            ganchado.HandleDesganchado();
            ganchado = null;
        }
    }

    /// <summary>
    /// Gancha um objeto ganhavel
    /// </summary>
    /// <param name="ganchavel">Objeto ganchavel</param>
    public void SetarGanchado(Ganchavel ganchavel) {
        ganchado = ganchavel;
        gancho.transform.SetParent(ganchavel.transform);
        ganchado.HandleGanchado();
    }

    /// <summary>
    /// Puxa o gancho
    /// </summary>
    public void PuxarGancho() {
        if (ganchado != null) {

            ganchado.HandlePuxado();

            Rigidbody rb = ganchado.GetComponent<Rigidbody>();

            if (rb != null) {
                float distancia = Vector3.Distance(ganchoSpawn.position, ganchado.meio);

                Vector3 direcao = (ganchoSpawn.position - ganchado.meio).normalized;
                Vector3 arremeco = Vector3.up * alturaDePuxada;

                rb.AddForce((direcao * distancia * forcaDePuxada) + arremeco, ForceMode.Impulse);
            }
        }
        
        if (gancho != null) DestruirGancho();
    }

}
