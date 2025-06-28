using UnityEngine;

public class LancaChamas : MonoBehaviour
{
    [SerializeField] private float maxComprimento = 8.0f;
    [SerializeField] private LayerMask layers;
    private Ray ray;
    private RaycastHit hitInfo;


    private void FixedUpdate()
    {
        CastColisao();   
    }

    private void CastColisao(){
        ray = new Ray(transform.position , transform.forward);

        if(Physics.SphereCast(transform.position, 1.0f, transform.forward, out hitInfo, maxComprimento,  layers)){
            
            // Debug.Log(hitInfo.transform.name);
            if(hitInfo.transform.CompareTag("Player")){
                hitInfo.transform.GetComponent<Player>().MudarVida(-1, "Lança-chamas");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.matrix = transform.localToWorldMatrix;
        float distance = hitInfo.distance;
        Gizmos.DrawWireCube(Vector3.zero + Vector3.forward * (distance/2 + 0.5f), new Vector3(1, 1, distance + 1));
    }

}
