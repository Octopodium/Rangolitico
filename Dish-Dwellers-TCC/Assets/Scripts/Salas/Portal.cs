using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Portal : IResetavel, SincronizaMetodo {

    [SerializeField] private bool finalDaDemo;
    [SerializeField] private GameObject canvasFinalDaDemo;

    List<Player> playersNoPortal = new List<Player>();
    [SerializeField] private Transform spawnDeSaida;
    Interagivel interagivel;
    
    public IndicadorFalso indicadorSaida;

    public bool semPreload = false;
    public string salaEscolhidaSemPre = "";

    void Awake() {
        interagivel = GetComponentInParent<Interagivel>();
    }

    void Start() {
        Sincronizavel sin = GetComponent<Sincronizavel>();
        if (sin == null) sin = gameObject.AddComponent<Sincronizavel>();
    }

    public override void OnReset() {
        playersNoPortal.Clear();
    }

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            PlayerEntra(other.gameObject);
        }
    }

    [Sincronizar]
    public void PlayerEntra(GameObject playerObj) {
        Player player = playerObj.GetComponent<Player>();
        if(player == null) return; // Se não for um player, não faz nada.
        if (playersNoPortal.Contains(player)) return;

        bool prosseguir = gameObject.Sincronizar(playerObj);
        if (!prosseguir) return;

        if (player.playerInput != null)
            player.playerInput.currentActionMap["Cancelar"].performed += SairDoPortal;
        
        playerObj.gameObject.SetActive(false);
        playersNoPortal.Add(player);

        player.indicador.Mostrar(interagivel, MotivoNaoInteracao.Cancelar);
        indicadorSaida.Copiar(player.indicador);
        
        // Caso os dois players tenham entrado na porta, passa de sala.
        PassarDeSala();
    }

    public void PassarDeSala() {
        if (playersNoPortal.Count < 2) return;
        Debug.Log("To passando de sala ein");
        foreach(Player player in playersNoPortal){
            player.indicador.Esconder(interagivel);
        }
        indicadorSaida.Esconder();
        if (finalDaDemo) VaiParaOFim();
        else if (!semPreload) GameManager.instance.PassaDeSala();
        else GameManager.instance.IrParaSalaSemPreload(salaEscolhidaSemPre);
    }


    public void SairDoPortal(InputAction.CallbackContext context) {
        SairDoPortal();
    }

    [Sincronizar]
    public void SairDoPortal(){
        if(playersNoPortal.Count == 1){
            gameObject.Sincronizar();
            

            Player player = playersNoPortal[0];

            player.indicador.Esconder(interagivel);
            indicadorSaida.Esconder();

            player.transform.position = spawnDeSaida.position + Vector3.up * 0.5f;    
            player.gameObject.SetActive(true);
            playersNoPortal.Remove(player);
            

            if (player.playerInput != null)
                player.playerInput.currentActionMap["Cancelar"].performed -= SairDoPortal;
        }
    }

    public string cenaDoFim = "Fim";

    public void VaiParaOFim() {
        GameManager.instance.HandleChegouNoFim();
        SceneManager.LoadScene(cenaDoFim, LoadSceneMode.Single);
    }

    public bool PlayerEstaDentro(Player p) {
        return playersNoPortal.Contains(p);
    }
}
