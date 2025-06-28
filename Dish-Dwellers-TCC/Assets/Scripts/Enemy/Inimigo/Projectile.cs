using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour {
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float lifeTime = 4.0f;
    [SerializeField] private float currentLifeTime;
    [SerializeField] private Player player;
    [SerializeField] private GameObject splashDeFogo; // Particula que é instanciada quando a bola explode.
    [SerializeField] private GameObject trail;
    [SerializeField] private VisualEffect trailFx;
    public GameObject owner;
    private Vector3 direction;
    public AudioClip audioClip;
    private bool isReflected = false;


    [Header("<color=green> Lima coisas :")]
    [SerializeField] private bool refletirNormal;


    void Start() {
        direction = transform.forward; //Usa a direção inicial do disparo
        currentLifeTime = lifeTime;
    }

    void FixedUpdate() {
        transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);

        if (currentLifeTime <= 0) {
            Destroy(gameObject);
        }
        currentLifeTime -= Time.fixedDeltaTime;
    }

    private void OnDestroy() {
        GameObject splash = Instantiate(splashDeFogo, transform.position, transform.rotation);
        Destroy(splash, 2.0f);

    }

    private void OnCollisionEnter(Collision other) {

        if (other.gameObject.CompareTag("Escudo") && !isReflected) {

            //Tenta pegar o centro da proteção (protecao) do escudo para refletir

            Escudo escudo = other.transform.GetComponentInParent<Escudo>();

            //CODIGO DO PEDRO DE LIMA:

            //Reseta o lifetime:
            currentLifeTime = lifeTime;

            transform.SetPositionAndRotation(escudo.pontoDeReflexao.position, escudo.pontoDeReflexao.rotation);
            direction = transform.forward;

            //FIM DO CÓDIGO DO PEDRO DE LIMA

            isReflected = true;

            /*
            Transform centroDoEscudo = other.transform; 

            if (escudo != null && escudo.protecao != null) {
                centroDoEscudo = escudo.protecao.transform;
            }

            Vector3 reflectDirection = centroDoEscudo.forward;
            reflectDirection.y = 0;
            reflectDirection = reflectDirection.normalized;

            
            Debug.DrawRay(centroDoEscudo.position, reflectDirection * 3, Color.cyan, 2f);

            direction = reflectDirection;
            isReflected = true;
            transform.rotation = Quaternion.LookRotation(reflectDirection);
            transform.position = centroDoEscudo.position + reflectDirection * 0.5f;
            */
        }

        else if (isReflected && other.gameObject == owner) {
            Debug.Log("Colidiu");

            //Quando acerta o proprietário do projetil(ou seja, a torreta) coloca o mesmo no estado de stunado
            InimigoTorreta torreta = owner.GetComponent<InimigoTorreta>();
            if (torreta != null) {
                torreta.GetStunned();
            }
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("Torreta") && !isReflected) {
            return;
        }

        else if (other.transform.CompareTag("Queimavel")) {
            other.transform.GetComponent<ParedeDeVinhas>().ReduzirIntegridade();
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("Player") && !isReflected) {
            Player player = other.transform.GetComponent<Player>();
            if (player != null) {
                player.MudarVida(-1);
                player.AplicarKnockback(transform, audioClip);
            }
            Destroy(gameObject);
        }
        
        else if (other.transform.CompareTag("Chao") || other.transform.CompareTag("Parede")) {
            Destroy(gameObject);
        }

        //previsão pra caso houver colisão com outros obstáculos
        else {
            Destroy(gameObject);
        }
    }
}