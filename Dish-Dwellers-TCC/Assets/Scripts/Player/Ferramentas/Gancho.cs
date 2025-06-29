using UnityEngine;
using System.Collections;

public class Gancho : MonoBehaviour, Ferramenta {
    public LineRenderer lineRenderer;
    public Transform ganchoSpawn;
    public GameObject ganchoPrefab;

    [Header("Configurações do Gancho")]
    public bool temMiraAuto = true;
    public float raioMiraAuto = 0.25f;
    protected Ganchavel alvoAuto;
    public LayerMask layerGancho;
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

    public bool acionada { get; protected set; } = false;
    bool acabouDePuxar = false;

    void FixedUpdate() {
        if (acionada && ganchado == null && gancho == null) {
            PreverGancho();
        }
    }

    /// <summary>
    /// Chamado no Awake do Player. Seta o jogador que está usando o gancho.
    /// </summary>
    /// <param name="jogador"></param>
    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    /// <summary>
    /// Chamado quando o jogador pressiona o botão de ação do gancho.
    /// </summary>
    public void Acionar() {
        if (acionada) return;

        if (ganchado != null) {
            PuxarGancho();
            acabouDePuxar = true;
        } else if (gancho == null) {
            this.jogador.MostrarDirecional(true);
        } else {
            DestruirGancho();
        }

        acionada = true;
    }

    /// <summary>
    /// Chamado quando o jogador solta o botão de ação do gancho.
    /// </summary>
    public void Soltar() {
        if (!acionada) return;

        this.jogador.MostrarDirecional(false);
        this.jogador.SetPontoFinal(false);
        this.jogador.linhaTrajetoria.enabled = false;

        if (gancho == null && ganchado == null && !acabouDePuxar) {
            AtirarGancho();
        }

        acionada = false;
        acabouDePuxar = false;
    }

    public void Cancelar() {
        if (!acionada) return;

        this.jogador.MostrarDirecional(false);

        if (ganchado != null) {
            ganchado.HandleDesganchado();
            ganchado = null;
        }

        if (gancho != null) {
            DestruirGancho();
        }

        acionada = false;
        acabouDePuxar = false;
    }

    /// <summary>
    /// Atira o gancho na direção que o jogador está olhando
    /// </summary>
    public void AtirarGancho() {
        gancho = Instantiate(ganchoPrefab, ganchoSpawn.position, Quaternion.identity);
        ProjetilDoGancho projetil = gancho.GetComponent<ProjetilDoGancho>();
        Vector3 direcao = jogador.direcao.normalized;

        if (temMiraAuto && alvoAuto != null) {
            Vector3 dirNova = alvoAuto.transform.position - ganchoSpawn.position;
            dirNova.y = jogador.direcao.y;
            direcao = dirNova.normalized;
        }

        projetil.Inicializar(this, direcao, velocidadeGancho);
    }

    /// <summary>
    /// Chama corotina que destroi o gancho (as vezes chamado pelo próprio projetil de gancho) [ver: DestruirGanchoCoroutine]
    /// </summary>
    public void DestruirGancho() {
        if (gancho != null) {
            StartCoroutine(DestruirGanchoCoroutine());
        }
    }

    /// <summary>
    /// Destroi o gancho (as vezes chamado pelo próprio projetil de gancho)
    /// </summary>
    public IEnumerator DestruirGanchoCoroutine() {
        if (gancho == null) yield break;

        if (ganchado != null) {
            IGanchavelAntesDesganchar ganchavelAntesDesganchar = ganchado.GetComponent<IGanchavelAntesDesganchar>();
            if (ganchavelAntesDesganchar != null) yield return ganchavelAntesDesganchar.GanchavelAntesDesganchar();
        }

        Destroy(gancho);
        lineRenderer.enabled = false;

        if (ganchado != null) {
            ganchado.HandleDesganchado();
            ganchado = null;
        }
    }

    /// <summary>
    /// Chama corotina que gancha um objeto ganchavel [ver: SetarGanchadoCoroutine]
    /// </summary>
    /// <param name="ganchavel">Objeto ganchavel</param>
    public void SetarGanchado(Ganchavel ganchavel) {
        StartCoroutine(SetarGanchadoCoroutine(ganchavel));
    }

    /// <summary>
    /// Gancha um objeto ganchavel
    /// </summary>
    /// <param name="ganchavel">Objeto ganchavel</param>
    public IEnumerator SetarGanchadoCoroutine(Ganchavel ganchavel) {
        IGanchavelAntesGanchar ganchavelAntesGanchar = ganchavel.GetComponent<IGanchavelAntesGanchar>();
        if (ganchavelAntesGanchar != null) yield return ganchavelAntesGanchar.GanchavelAntesGanchar();

        ganchado = ganchavel;
        gancho.transform.SetParent(ganchavel.transform);
        ganchado.HandleGanchado();
    }

    /// <summary>
    /// Chama corotina que puxa o gancho [ver: PuxarGanchoCoroutine]
    /// </summary>
    public void PuxarGancho() {
        StartCoroutine(PuxarGanchoCoroutine());
    }


    /// <summary>
    /// Puxa o gancho (e por consequência o objeto ganchado) em direção ao inicio da corda do gancho.
    /// </summary>
    public virtual IEnumerator PuxarGanchoCoroutine() {
        if (ganchado != null) {
            IGanchavelAntesPuxar ganchavelAntesPuxar = ganchado.GetComponent<IGanchavelAntesPuxar>();
            if (ganchavelAntesPuxar != null) yield return ganchavelAntesPuxar.GanchavelAntesPuxar();

            ganchado.HandlePuxado();

            Rigidbody rb = ganchado.GetComponent<Rigidbody>();

            if (rb != null) {
                float distancia = Vector3.Distance(ganchoSpawn.position, ganchado.meio);

                Vector3 direcao = (ganchoSpawn.position - ganchado.meio).normalized;
                Vector3 arremeco = Vector3.up * alturaDePuxada;
                Vector3 puxada = (direcao * distancia * forcaDePuxada) + arremeco;

                rb.AddForce(puxada * rb.mass, ForceMode.Impulse);
            }

            ganchado.HandleDesganchado();
            ganchado = null;
        }

        if (gancho != null) DestruirGancho();

        yield return null;
    }


    public void PreverGancho() {
        if (gancho != null) return;

        Vector3 posicaoInicial = ganchoSpawn.position;
        Vector3 direcao = jogador.direcao.normalized;
        float distanciaMaxima = this.distanciaMaxima;

        jogador.linhaTrajetoria.enabled = true;
        jogador.linhaTrajetoria.positionCount = 2;
        jogador.linhaTrajetoria.SetPosition(0, posicaoInicial);

        Vector3 posicaoFinal;
        
        RaycastHit hit;
        Collider hitted = null;
        if (Physics.Raycast(posicaoInicial, direcao, out hit, distanciaMaxima, layerGancho)) {
            posicaoFinal = hit.point;
            hitted = hit.collider;
        } else {
            posicaoFinal = posicaoInicial + direcao * distanciaMaxima;
        }

        posicaoFinal = AutoAim(posicaoFinal, out bool encontrou, raioMiraAuto, hitted);
        
        this.jogador.SetPontoFinal(true, posicaoFinal, encontrou);
        jogador.linhaTrajetoria.SetPosition(1, posicaoFinal);
    }

    RaycastHit[] hits = new RaycastHit[24];
    public Vector3 AutoAim(Vector3 fimPos, out bool encontrou, float radius = 0.25f, Collider hitted = null) {
        encontrou = false;
        if (!temMiraAuto) return fimPos;

        if (hitted != null) {
            Ganchavel alvoRecebido = hitted.GetComponent<Ganchavel>();
            if (alvoRecebido != null) {
                encontrou = true;
                alvoAuto = alvoRecebido;
                return alvoRecebido.meio;
            }
        }

        Vector3 direcao = jogador.direcao.normalized;
        float distanciaMaxima = Vector3.Distance(ganchoSpawn.position, fimPos);

        int hitCount = Physics.SphereCastNonAlloc(fimPos, radius, -direcao, hits, distanciaMaxima, layerGancho);
        Debug.DrawLine(fimPos, fimPos - direcao * distanciaMaxima, Color.green, 0.1f);

        if (hitCount > 0) {
            float maisLonge = 0f;
            Ganchavel alvo = null;


            for (int i = 0; i < hitCount; i++) {
                RaycastHit hit = hits[i];
                if (hit.collider != null) {
                    Ganchavel ganchavel = hit.collider.GetComponent<Ganchavel>();
                    if (ganchavel != null) {
                        Debug.DrawLine(ganchoSpawn.position, ganchavel.meio, Color.magenta, 0.1f);
                        float distancia = Vector3.Distance(ganchoSpawn.position, ganchavel.meio);
                        if (distancia > maisLonge && distancia <= distanciaMaxima) {
                            maisLonge = distancia;
                            alvo = ganchavel;
                        }
                    }
                }
            }

            if (alvo != null) {
                alvoAuto = alvo;
                encontrou = true;
                return alvo.meio;
            }
        }

        alvoAuto = null;
        return fimPos;
    }
}
