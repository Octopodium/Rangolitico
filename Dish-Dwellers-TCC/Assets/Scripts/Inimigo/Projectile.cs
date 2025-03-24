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

    public void MudarDirecao(Vector2 direcao) {
        MudarDirecao(new Vector3(direcao.x, 0, direcao.y));
    }

    public void MudarDirecao(Vector3 direcao) {
        this.direcao = direcao;
    }

    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Refletivel")) {
            Vector3 pontoDeColisao = other.ClosestPoint(transform.position);
            Vector3 normal = (transform.position - pontoDeColisao).normalized;
            Vector3 novaDir = -Vector3.Reflect(direcao, normal);
            MudarDirecao(novaDir);
        } else if(other.CompareTag("Player")) {
            Player player = other.GetComponent<Player>(); 
            player.playerVidas -= 1;
            Destroy(gameObject);
            Debug.Log("Player levou dano");
        }
    }
}
