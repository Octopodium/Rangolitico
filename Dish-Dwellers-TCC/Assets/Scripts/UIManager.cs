using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    //Evento de dano (atualizar display)
    //Evento de pontuacao (atualizar display)

    /// <summary>
    /// SceneManager.LoadScene generico, recebe uma string
    /// </summary>
    public void CarregarCena(string nomeCena){
        SceneManager.LoadScene(nomeCena);
    }

    /// <summary>
    /// Toggle qualquer GameObject, sempre da SetActive no contrario da hierarquia
    /// </summary>
    public void AtivarEDesativarObjeto(GameObject objeto){
        objeto.SetActive(!objeto.activeInHierarchy);
    }

    public void QuitJogo(){
        Application.Quit();
    }
}
