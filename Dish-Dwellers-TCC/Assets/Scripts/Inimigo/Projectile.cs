using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 7f;
    Player player;
    void Start()
    {
        Destroy(gameObject, 2.0f);
    }

    void FixedUpdate()
    {
        transform.Translate(0, 0, projectileSpeed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>(); 
            player.playerVidas -= 1;
            Destroy(gameObject);
            Debug.Log("Player levou dano");
        }
    }
}
