using UnityEngine;
using UnityEngine.Events;

public class Porta : IResetavel, Interacao{
    [Tooltip ("Colisor que transporta o jogador quando destrancada")]
    [SerializeField]private GameObject portal;
    [SerializeField] private Animator animator;
    private bool destrancada;
    public bool trancada => !destrancada;
    public UnityEvent OnDestrancaPorta;
    


    private void Start() {
        portal.SetActive(false);
    }

    public override void OnReset() {
        Trancar();
        portal.GetComponent<Portal>().OnReset();
    }

    public void Interagir(Player jogador){
        if(destrancada){
            return;
        }

        if(jogador.carregando != null && jogador.carregando.CompareTag("Chave")){
            Destrancar();
            // Retira a chave do jogador.
            
            jogador.carregando.gameObject.SetActive(false);
            jogador.carregador.carregado = null;
        }
    }

    private void AbrePorta() {
        animator.SetTrigger("AbrePorta");
    }

    public void Destrancar() {
        AbrePorta();
        portal.SetActive(true);

        OnDestrancaPorta?.Invoke();

        // Previne que o jogador possa destrancar a porta duas vezes.
    }

    public void Trancar() {
        portal.SetActive(false);
    }
}
