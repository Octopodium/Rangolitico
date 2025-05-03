using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 7f;
    public GameObject owner;
    public Player player; 
    private Vector3 direction;
    private bool isReflected = false;
    [SerializeField] private AnimatorTorreta animator;


    [Header("<color=green> Lima coisas :")]
    [SerializeField]private bool refletirNormal;

    void Awake()
    {
        animator = GetComponentInChildren<AnimatorTorreta>();
    }
    
    void Start()
    {
        direction = transform.forward; //Usa a direção inicial do disparo
        Destroy(gameObject, 4f);
    }

    void FixedUpdate()
    {
        transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Escudo") && !isReflected)
        {
            Vector3 reflectDirection;

            if(refletirNormal) 
                reflectDirection = other.transform.forward;
            else
                reflectDirection = (owner.transform.position - transform.position).normalized;

            reflectDirection.y = 0;
            direction = reflectDirection;
            isReflected = true;
            
            transform.rotation = Quaternion.LookRotation(reflectDirection);
        }

        else if (isReflected && other.gameObject == owner)
        {
            Debug.Log("Colidiu");
            //Quando acerta o proprietário do projetil(ou seja, a torreta) coloca o mesmo no estado de stunado
            InimigoTorreta torreta = owner.GetComponent<InimigoTorreta>(); 
            if (torreta != null)
            {
                torreta.GetStunned();
            }
            Destroy(gameObject);
        }

        else if(other.gameObject.CompareTag("Torreta") && !isReflected)
        {
            return;
        }

        else if(other.CompareTag("Queimavel"))
        {
            other.GetComponent<ParedeDeVinhas>().ReduzirIntegridade();
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("Player") && !isReflected)
        {
            player = other.GetComponent<Player>();
            player.MudarVida(-1);
            Debug.Log("deu dano");
            Destroy(gameObject);
        }

        //previsão pra caso houver colisão com outros obstáculos
        else
        {
            Destroy(gameObject);
        }
    }
}