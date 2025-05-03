using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Portal : IResetavel{

    [SerializeField] private bool finalDaDemo;
    [SerializeField] private GameObject canvasFinalDaDemo;

    List<Player> playersNoPortal = new List<Player>();
    [SerializeField] private Transform spawnDeSaida;


    public override void OnReset(){
        playersNoPortal.Clear();
    }

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            Player player = other.GetComponent<Player>();
            if(player == null) return; // Se não for um player, não faz nada.
            player.inputActionMap["Cancelar"].performed += SairDoPortal;
            
            other.gameObject.SetActive(false);

            if (playersNoPortal.Contains(player)) return; // Se o player já estiver na lista, não adiciona novamente.
            playersNoPortal.Add(player);

            // Caso os dois players tenham entrado na porta, passa de sala.
            if(playersNoPortal.Count > 1){
                if(finalDaDemo) canvasFinalDaDemo.SetActive(true);
                else GameManager.instance.PassaDeSala();
            }

            Debug.Log("Players no portal : " + playersNoPortal.Count);
        }
    }

    private void SairDoPortal(InputAction.CallbackContext context){
        if(playersNoPortal.Count == 1){
            Player player = playersNoPortal[0];

            player.transform.position = spawnDeSaida.position;
            player.gameObject.SetActive(true);
            playersNoPortal.Remove(player);

            Debug.Log("<color=red>Saiu do portal.");

            player.inputActionMap["Cancelar"].performed -= SairDoPortal;
        }
    }

}
