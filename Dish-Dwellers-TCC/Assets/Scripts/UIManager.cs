using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] GameObject[] coracoesEsquerda;
    [SerializeField] GameObject[] coracoesDireita;

    void Awake(){
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
        // DontDestroyOnLoad(gameObject);
        Player.OnVidaMudada += HandleDisplayVida;
    }

    private void OnDestroy(){
        Player.OnVidaMudada -= HandleDisplayVida;
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

    public void QuitJogo(){
        Application.Quit();
    }
}
