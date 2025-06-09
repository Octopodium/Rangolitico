using UnityEngine;

public class LimitadorDeQueda : MonoBehaviour
{
    private void OnTriggerEnter(Collider other){
        if (other.CompareTag("Player")) {
            Player player = other.GetComponent<Player>();
            if (player != null) player.Morrer();
        }

        Destrutivel destrutivel = other.GetComponent<Destrutivel>();
        if(destrutivel){
            destrutivel.OnDestruido.Invoke();
        }
    }
}
