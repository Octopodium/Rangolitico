using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour{

    private int playersNoPortal = 0; // Conta quantos players entraram no portal.

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            other.gameObject.SetActive(false);

            // Caso os dois players tenham entrado na porta, passa de sala.
            if(++playersNoPortal > 0){
                GameManager.instance.PassaDeSala();
            }
            Debug.Log("Players no portal : " + playersNoPortal);
        }
    }

}
