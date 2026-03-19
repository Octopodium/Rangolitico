using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Sincronizavel))]
public class Projectile : MonoBehaviour, SincronizaMetodo {
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float lifeTime = 4.0f;
    [SerializeField] private float currentLifeTime;
    [SerializeField] private Player player;
    [SerializeField] private GameObject splashDeFogo; // Particula que é instanciada quando a bola explode.
    [SerializeField] private GameObject trail;
    [SerializeField] private VisualEffect trailFx;
    [SerializeField] private GameObject decalQueimado;
    public GameObject owner;
    private Vector3 direction;
    
    public AudioClip knockBackSom;
    public AudioClip vinhaQueimandoSom;
    private bool isReflected = false;

    Vector3 posCatchUp;
    float catchupCounter = 0.0f;
    float catchupTime = 0.01f;


    [Header("<color=green> Lima coisas :")]
    [SerializeField] private bool refletirNormal;

    void Start() {
        direction = transform.forward; //Usa a direção inicial do disparo
        currentLifeTime = lifeTime;
    }

    public void SetDir(Vector3 dir) {
        direction = dir;
    }

    void FixedUpdate() {
        transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);

        if (GameManager.instance.isOnline){
            if (GameManager.instance.isServer) SetPosCatchUp(transform.position, false);
            else transform.position = posCatchUp;
        }

        if (currentLifeTime <= 0) {
            Destroy(gameObject);
        }
        currentLifeTime -= Time.fixedDeltaTime;
    }

    private void OnDestroy() {
        GameObject splash = Instantiate(splashDeFogo, transform.position, transform.rotation);
        Destroy(splash, 2.0f);

    }

    private void DeixarMarcaDeQueimado(Vector3 contato, Vector3 normal) {
        GameObject decal = Instantiate(decalQueimado);
        decal.transform.position = contato;
        decal.transform.forward = normal;
    }

    [Sincronizar]
    public void Refletir(Vector3 pos, Quaternion rot) {
        gameObject.Sincronizar(pos, rot);

        //CODIGO DO PEDRO DE LIMA:

        //Reseta o lifetime:
        currentLifeTime = lifeTime;

        transform.SetPositionAndRotation(pos, rot);
        direction = transform.forward;

        //FIM DO CÓDIGO DO PEDRO DE LIMA

        AudioManager.PlaySounds(TiposDeSons.SHIELDHIT);
        isReflected = true;
    }

    private void OnCollisionEnter(Collision other) {
        GameObject objeto = other.gameObject;
        ContactPoint contact = other.GetContact(0);

        if (!GameManager.instance.isOnline || objeto.GetComponent<Sincronizavel>() != null || objeto.GetComponent<SubSincronizavel>() != null)
            LidarComColisao(objeto.tag, contact.point, contact.normal, objeto, false);
        else
            LidarComColisaoSemObjeto(objeto.tag, contact.point, contact.normal);
    }

    [Sincronizar]
    public void LidarComColisaoSemObjeto(string tag, Vector3 contact, Vector3 contactNormal) {
        gameObject.Sincronizar(tag, contact, contactNormal);
        LidarComColisao(tag, contact, contactNormal, null, true);
    }

    [Sincronizar]
    public void LidarComColisao(string tag, Vector3 contact, Vector3 contactNormal, GameObject objeto, bool redirecionado) {
        if (!redirecionado)
            gameObject.Sincronizar(tag, contact, contactNormal, objeto, false);

        // Debug.Log("Colidiu com: " + tag + " " + contact + " "+ contactNormal + " "+ objeto + " ");

        if (tag == "Escudo") {
            //Tenta pegar o centro da proteção (protecao) do escudo para refletir 
            Debug.Log("ref");
            if (!isReflected) {
                Escudo escudo = objeto.transform.GetComponentInParent<Escudo>();

                Refletir(escudo.pontoDeReflexao.position, escudo.pontoDeReflexao.rotation);
            }
           
        }

        else if (isReflected && objeto != null && objeto == owner) {
            Debug.Log("Colidiu");

            //Quando acerta o proprietário do projetil(ou seja, a torreta) coloca o mesmo no estado de stunado
            InimigoTorreta torreta = owner.GetComponent<InimigoTorreta>();
            if (torreta != null) {
                torreta.GetStunned();
            }
            Destroy(gameObject);
        }

        else if (tag == "Torreta" && !isReflected) {
            return;
        }

        else if (tag == "Queimavel") {
            objeto.transform.GetComponent<ParedeDeVinhas>().ReduzirIntegridade(transform.position);
            Destroy(gameObject);
        }

        else if (tag == "Player" && !isReflected) {
            Player player = objeto.transform.GetComponent<Player>();
            if (player != null && (!GameManager.instance.isOnline || GameManager.instance.isServer)) {
                player.MudarVida(-1, AnimadorPlayer.fonteDeDano.FOGO);
                player.AplicarKnockback(transform);
                AudioManager.PlaySounds(TiposDeSons.KNOCKBACK);
            }
            Destroy(gameObject);
        }
        
        else if (tag == "Chao" ||  tag == "Parede") {
            DeixarMarcaDeQueimado(contact, contactNormal);
            Destroy(gameObject);
        }

        //previsão pra caso houver colisão com outros obstáculos
        else {
            DeixarMarcaDeQueimado(contact, contactNormal);
            Destroy(gameObject);
        }
    }

    [Sincronizar]
    public void SetPosCatchUp(Vector3 pos, bool bypassCheck) {
        if (!GameManager.instance.isOnline) return;
        if (!bypassCheck && catchupCounter < catchupTime) {
            gameObject.Sincronizar(pos, false);
            catchupCounter += Time.fixedDeltaTime;
            return;
        }

        gameObject.Sincronizar(pos, true);
        catchupCounter = 0;
        posCatchUp = transform.position;
    }
}