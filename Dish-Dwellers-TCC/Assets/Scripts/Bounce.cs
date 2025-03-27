using UnityEngine;

public class Bounce : MonoBehaviour
{
    private void FixedUpdate(){
        Vector3 posicao = transform.position;

        posicao.y -= Mathf.Sin((Time.time - 0.5f) * Mathf.PI);
        transform.position = posicao;
    }
}
