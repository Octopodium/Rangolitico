using UnityEngine;

public class LimitadorDeQueda : MonoBehaviour
{
    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            GameManager.instance.ResetSala();
        }
    }
}
