using UnityEngine;

public class Porta : MonoBehaviour, Interacao{
    [Tooltip ("Colisor que transporta o jogador quando destrancada")]
    [SerializeField]private GameObject portal;
    private bool destrancada;

    private void Start(){
        portal.SetActive(false);
    }

    public void Interagir(Player jogador){
        if(destrancada){
            return;
        }

        if(jogador.carregando != null && jogador.carregando.CompareTag("Chave")){
            Destrancar();
            // Retira a chave do jogador.
            Destroy(jogador.carregando.gameObject);
        }
    }

    private void Destrancar(){
        portal.SetActive(true);

        // Previne que o jogador possa destrancar a porta duas vezes.
        
    }
}
