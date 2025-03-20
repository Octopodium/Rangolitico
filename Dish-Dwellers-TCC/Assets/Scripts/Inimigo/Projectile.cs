using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 7f;
    Player player;
    Vector3 direcao = new Vector3(0,0,1);

    void Start()
    {
        Destroy(gameObject, 2.0f);
    }

    void FixedUpdate()
    {
        transform.Translate(direcao * projectileSpeed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Refletivel")) {
            Vector3 normal = other.transform.forward;
            Vector3 novaDir = Vector3.Reflect(direcao, normal);
            direcao = novaDir;
        } else if(other.CompareTag("Player")) {
            Player player = other.GetComponent<Player>(); 
            player.playerVidas -= 1;
            Destroy(gameObject);
            Debug.Log("Player levou dano");
        } 
    }
}
