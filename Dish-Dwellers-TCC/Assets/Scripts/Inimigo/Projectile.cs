using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 7f;
    public GameObject owner;
    public Player player; 
    private Vector3 direction;
    private bool isReflected = false;

    [Header("<color=green> Lima coisas :")]
    [SerializeField]private bool refletirNormal;

    void Awake()
    {
        player = GameObject.FindObjectOfType<Player>();
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
        //Se colidir com o inimigo após ser refletido
        else if (isReflected && other.gameObject == owner)
        {
            Destroy(other.gameObject); // Destrói o inimigo (ou aplica dano)
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Player") && !isReflected)
        {
            player.MudarVida(-1);
            Debug.Log("deu dano");
        }
        //previsão pra caso houver colisão com outros obstáculos
        else
        {
            Destroy(gameObject);
        }
    }
}