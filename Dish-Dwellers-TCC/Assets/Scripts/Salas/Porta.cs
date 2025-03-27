using UnityEngine;

public class Porta : MonoBehaviour, Interacao{
    [Tooltip ("Colisor que transporta o jogador quando destrancada")]
    [SerializeField]private GameObject portal;

    private void Start(){
        portal.SetActive(false);
    }

    public void Interagir(Player jogador){
        if(jogador.carregando.CompareTag("Chave")){
            Destrancar();
            // Retira a chave do jogador.
            jogador.carregador.gameObject.SetActive(false);
        }
    }

    private void Destrancar(){
        portal.SetActive(true);

        // Previne que o jogador possa destrancar a porta duas vezes.
        this.enabled = false;
    }
}
