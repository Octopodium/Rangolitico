using UnityEngine;
using UnityEngine.Events;

public class Porta : MonoBehaviour, Interacao{
    [Tooltip ("Colisor que transporta o jogador quando destrancada")]
    [SerializeField]private GameObject portal;
    private bool destrancada;
    public UnityEvent OnDestrancaPorta;


    [Space(15)][Header("<color=yellow>Gambiarra pra fazer a porta mudar de cor")]
    [SerializeField] private Renderer portalRenderer;
    private MaterialPropertyBlock mpb;

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

        MudaCorDoPortal(Color.green);

        OnDestrancaPorta?.Invoke();

        // Previne que o jogador possa destrancar a porta duas vezes.
        
    }

    private void MudaCorDoPortal(Color color){
        mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", color);
        portalRenderer.SetPropertyBlock(mpb);
    }
}
