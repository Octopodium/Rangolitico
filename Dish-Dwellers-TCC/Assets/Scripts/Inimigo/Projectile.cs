using UnityEngine;

public class Projectile : MonoBehaviour
{
   [SerializeField] private float projectileSpeed = 7f;

    void Start()
    {
        Destroy(gameObject, 2.0f);
    }

    void FixedUpdate()
    {
        transform.Translate(0, 0, projectileSpeed * Time.fixedDeltaTime);
    }
}
