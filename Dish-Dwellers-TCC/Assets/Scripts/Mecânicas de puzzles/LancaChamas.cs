using UnityEngine;

public class LancaChamas : MonoBehaviour
{
    private RaycastHit[] colisores = new RaycastHit[2];
    [SerializeField] private float maxComprimento = 8.0f;
    [SerializeField] private LayerMask layers;
    private Ray ray;


    private void FixedUpdate()
    {
        CastColisao();   
    }

    private void CastColisao(){
        ray = new Ray(transform.position , transform.forward);

        if(Physics.SphereCast(ray, 1.0f, out RaycastHit hitInfo, maxComprimento,  layers)){
            
            Debug.Log(hitInfo.transform.name);
            if(hitInfo.transform.CompareTag("Player")){
                hitInfo.transform.GetComponent<Player>().MudarVida(-1);
                Debug.Log("player acertado.");
                Debug.Log(hitInfo.distance);
            }
        }
    }

}
