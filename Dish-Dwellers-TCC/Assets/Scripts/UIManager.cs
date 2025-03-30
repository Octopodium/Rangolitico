using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject[] coracoesEsquerda;
    [SerializeField] GameObject[] coracoesDireita;
    public GameObject telaPause;

    void Awake(){
        Player.OnVidaMudada += HandleDisplayVida;
        GameManager.OnPause += HandlePausa;
    }

    private void OnDestroy(){
        Player.OnVidaMudada -= HandleDisplayVida;
        GameManager.OnPause -= HandlePausa;
    }

    /// <summary>
    /// Toggle qualquer GameObject, sempre da SetActive no contrario da hierarquia
    /// </summary>

    public void AtivarEDesativarObjeto(GameObject objeto){
        objeto.SetActive(!objeto.activeInHierarchy);
    }

    public void HandleDisplayVida(Player player, int valor){
        GameObject[] coracoes = player.qualPlayer == QualPlayer.Player1 ? coracoesEsquerda : coracoesDireita;

        for (int i = 0; i < coracoesEsquerda.Length; i++){ 
            //percorre pela array de coracoes e ativa caso ele for menor que as vidas, ele ativa
            //como temos 3 de vida e a array tem 0,1,2 ele trata por i e nao pelo numero de vida
            coracoesEsquerda[i].SetActive(i < player.playerVidas);
        }
    }

    public void HandlePausa(bool estado){
        AtivarEDesativarObjeto(telaPause);
    }

    public void DespauseNoResume(){ 
    //Juan eu preciso que o botao Resumo funcione de algum jeito
    //colocando o GM no OnClick ou fazendo uma
    //funcao, escolhi a funcao pois posso comentar
        if(GameManager.instance != null){
            GameManager.instance.Pause();
        }
    }

    public void QuitJogo(){
        Application.Quit();
    }
}
