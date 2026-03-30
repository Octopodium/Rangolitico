using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Sincronizavel))]
public class InimigoTorreta : Inimigo, SincronizaMetodo
{
    #region Declarações de variáveis

    [Header("Referências de ação do inimigo")] [Space(10)]
    public GameObject projectile;
    public Transform fireAction;

    [Header("Valores de ação do inimigo torreta")] [Space(10)]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;
    [SerializeField] Vector3 direction;

    [Header("Foco do alvo")]
    [SerializeField] private float tempoDeFoco = 3f;
    private float tempoRestanteDeFoco;
    private bool temAlvoFixo = false;

    [Header("Configurações de Stun")]
    [SerializeField] private float stunDuration = 3f;
    private bool isStunned = false;
    private float stunTimer = 0f;

    [Header("Animação :")]
    [SerializeField] private AnimatorTorreta animator;
    [Tooltip("Tempo que demora para cospir a bola de fogo depois de iniciar a animação")]
    [SerializeField] private float delayDoTiro;

    AudioSource audioSource;

    [Header("AudioClips da Torreta")]
    [SerializeField] AudioClip[] audioClips;

    Sincronizavel sincronizavel;

    #endregion

    private void Awake() {
        animator = GetComponentInChildren<AnimatorTorreta>();
        audioSource = GetComponentInChildren<AudioSource>();
        sincronizavel = GetComponent<Sincronizavel>();
        sincronizavel.AposSetup(() => sincronizavel.onObjetoSpawnado += AposSpawnTiro);
    }

    private void Start()
    {
        target = EncontrarPlayerMaisProximo();
        if (target != null)
        {
            temAlvoFixo = true;
            tempoRestanteDeFoco = tempoDeFoco;
        }
    }

    private void FixedUpdate()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                animator.Atordoado(false);
            }
            return; // Pra não atacar enquanto estiver stunado, ta meio ruim eu sei, eu sei...
        }

        base.ChecagemDeZonas();
        AtualizarAlvo();
        MovimentoPerseguir();
        Atacar();
    }

    protected override void MovimentoPerseguir()
    {        
        if(_playerNoCampoDeVisao && target != null)
        {
            direction = target.position - transform.position;
            animator.Olhar(direction);
            direction.y = 0;
        }
    }

    public override void Atacar()
    {
        if (!isStunned && _playerNaZonaDeAtaque && target != null && Time.time > nextFire) {
            /*
            nextFire = Time.time + fireRate;
            animator.Cospe();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            GameObject newProjectile = Instantiate(projectile, fireAction.transform.position, targetRotation);
            newProjectile.GetComponent<Projectile>().owner = this.gameObject;
            base.Atacar();
            */
            StartCoroutine(Cospir(delayDoTiro));
        }
    }

    IEnumerator Cospir(float tempoParaCospir) {
        nextFire = Time.time + fireRate;
        animator.Cospe();
        audioSource.clip = audioClips[Random.Range(0, 3)];
        audioSource.Play();

        //Sincroniza a animação e a criação do projetil
        yield return new WaitForSeconds(tempoParaCospir);

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        sincronizavel.AposSetup(() => {
            sincronizavel.Spawnar(projectile, fireAction.transform.position, targetRotation);
            base.Atacar();
        });
    }


    void AposSpawnTiro(GameObject tiro) {
        Debug.Log(tiro?.name, tiro);
        if (tiro != null) {
            tiro.transform.LookAt(target);
            Projectile proj = tiro.GetComponent<Projectile>();
            proj.owner = this.gameObject;
            proj.SetDir(proj.transform.forward);
        }
    }

     /// <summary>
    /// Método para checar transform mais próximo dentro da zona de percepção da torreta
    /// faz o calculo do player que está em menor distancia baseado na interação com as zonas de 
    /// percepção, retornar o player mais próximo na área para poder focar nele por um periodo de tempo.
    /// </summary>
    /// <returns></returns>
    private Transform EncontrarPlayerMaisProximo() {
        Transform maisProximo = null;
        float menorDistancia = Mathf.Infinity;

        foreach (var jogador in GameManager.instance.jogadores) {
            float distancia = Vector3.Distance(transform.position, jogador.transform.position);
            if (distancia <= zonaDeAtaque && distancia < menorDistancia) {
                menorDistancia = distancia;
                maisProximo = jogador.pontoCentral.transform;
            }
        }

        return maisProximo;
    }

    /// <summary>
    /// Método reponsável por fazer a troca de target entre os players, quando o tempo de foco 
    /// do inimigo termina, ele procura um novo target dentro da sua zona de percepção ou ataque.
    /// </summary>
    private void AtualizarAlvo()
    {
        if (temAlvoFixo)
        {
            tempoRestanteDeFoco -= Time.deltaTime;
            if (tempoRestanteDeFoco <= 0f || target == null || !PlayerNaZona(target))
            {
                temAlvoFixo = false;
            }
        }

        if (!temAlvoFixo)
        {
            Transform novoAlvo = EncontrarPlayerMaisProximo();
            if (novoAlvo != null)
            {
                target = novoAlvo;
                temAlvoFixo = true;
                tempoRestanteDeFoco = tempoDeFoco;
            }
            else
            {
                target = null;
            }
        }
    }

    private bool PlayerNaZona(Transform jogador)
    {
        return Vector3.Distance(transform.position, jogador.position) <= zonaDeAtaque;
    }
    
    protected override void TomaDano(int valor)
    {
        base.TomaDano(valor);
    }

    /// <summary>
    /// Chamada do animator da torreta que coloca o inimigo no estado de stun
    /// assim que ele for atingido por um projetil refletido.
    /// </summary>
    [Sincronizar]
    public void GetStunned() {
        gameObject.Sincronizar();
        Debug.Log("STUNA!!!");
        isStunned = true;
        stunTimer = stunDuration;
        animator.Atordoado(true);
    }

    public void TorretaAgarrada() {
        if (isStunned) {
            animator.Agarrado(true);
        }
    }

    public void MorteDaTorreta() {
        animator.Morre();
    }

    /// <summary>
    /// Só pra visualização das zonas de percepção e ataque do inimigo na cena.
    /// </summary>
    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, campoDeVisao);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, zonaDeAtaque);
    }
}
