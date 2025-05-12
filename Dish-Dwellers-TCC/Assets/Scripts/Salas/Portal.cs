using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Portal : IResetavel, SincronizaMetodo {

    [SerializeField] private bool finalDaDemo;
    [SerializeField] private GameObject canvasFinalDaDemo;

    List<Player> playersNoPortal = new List<Player>();
    [SerializeField] private Transform spawnDeSaida;

    void Start() {
        Sincronizavel sin = GetComponent<Sincronizavel>();
        if (sin == null) sin = gameObject.AddComponent<Sincronizavel>();
    }

    public override void OnReset(){
        playersNoPortal.Clear();
    }

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            PlayerEntra(other.gameObject);
        }
    }

    [Sincronizar("entra-portal")]
    public void PlayerEntra(GameObject playerObj) {
        Player player = playerObj.GetComponent<Player>();
        if(player == null) return; // Se não for um player, não faz nada.

        gameObject.Sincronizar("entra-portal", playerObj);

        player.inputActionMap["Cancelar"].performed += SairDoPortal;
        
        playerObj.gameObject.SetActive(false);

        if (playersNoPortal.Contains(player)) return; // Se o player já estiver na lista, não adiciona novamente.
        playersNoPortal.Add(player);

        // Caso os dois players tenham entrado na porta, passa de sala.
        if(playersNoPortal.Count > 1){
            if(finalDaDemo) canvasFinalDaDemo.SetActive(true);
            else GameManager.instance.PassaDeSala();
        }

        Debug.Log("Players no portal : " + playersNoPortal.Count);
    }


    public void SairDoPortal(InputAction.CallbackContext context) {
        SairDoPortal();
    }

    [Sincronizar("sair-portal")]
    public void SairDoPortal(){
        if(playersNoPortal.Count == 1){
            gameObject.Sincronizar("sair-portal");
            

            Player player = playersNoPortal[0];

            player.transform.position = spawnDeSaida.position;
            player.gameObject.SetActive(true);
            playersNoPortal.Remove(player);

            Debug.Log("<color=red>Saiu do portal.");

            player.inputActionMap["Cancelar"].performed -= SairDoPortal;
        }
    }

}
