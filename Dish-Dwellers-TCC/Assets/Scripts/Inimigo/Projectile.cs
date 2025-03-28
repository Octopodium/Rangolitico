using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 7f;
    public GameObject owner; 
    private Vector3 direction;
    private bool isReflected = false;

    void Start()
    {
        direction = transform.forward; //Usa a direção inicial do disparo
        Destroy(gameObject, 4f);
    }

    void FixedUpdate()
    {
        transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Escudo") && !isReflected)
        {
            Vector3 reflectDirection = (owner.transform.position - transform.position).normalized;
            direction = reflectDirection;
            isReflected = true;
            
            // Reorienta o projétil para mirar no inimigo
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
            Destroy(gameObject);
        }
        //previsão pra caso houver colisão com outros obstáculos
        else if (other.gameObject.CompareTag("Parede"))
        {
            Destroy(gameObject);
        }
    }
}