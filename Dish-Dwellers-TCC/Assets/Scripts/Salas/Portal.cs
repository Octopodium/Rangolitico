using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour{

    private int playersNoPortal = 0; // Conta quantos players entraram no portal.
    [SerializeField] private bool finalDaDemo;
    [SerializeField] private GameObject canvasFinalDaDemo;

    private void OnTriggerEnter(Collider other){
        if(finalDaDemo){
            canvasFinalDaDemo.SetActive(true);
        }

        if(other.CompareTag("Player")){
            other.gameObject.SetActive(false);

            // Caso os dois players tenham entrado na porta, passa de sala.
            if(++playersNoPortal > 1){
                GameManager.instance.PassaDeSala();
            }
            Debug.Log("Players no portal : " + playersNoPortal);
        }
    }

}
