using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public Actions input;

    public static event UnityAction<bool> OnPause;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);

        input = new Actions();
        input.Enable();

        input.UI.Pause.started += ctx => Pause();
        
        GetPlayers();
    }

    public void Pause(){
        if(Time.timeScale == 1){
            OnPause?.Invoke(true);
            Time.timeScale = 0;
        }else{
            OnPause?.Invoke(false);
            Time.timeScale = 1;
        }
    }
    
    #region Sistema de salas
    public List<Player> jogadores = new List<Player>();
    private AsyncOperation cenaProx;
    private sala sala;

    
    /// <summary>
    /// Descarrega a sala atual, finaliza o carregamento da proxima e posiciona o jogador no porximo ponto de spawn.
    /// </summary>
    public void PassaDeSala(){
        cenaProx.allowSceneActivation = true;
    }

    /// <summary>
    /// Reinicia a sala para as condições iniciais.
    /// </summary>
    public void ResetSala(){
        sala.PosicionarJogador();
    }

    // Metodo lento para encontrar os jogadores
    private void GetPlayers(){
        foreach( var data in GameObject.FindGameObjectsWithTag("Player")){
            jogadores.Add(data.GetComponent<Player>());
        }
    }

    /// <summary>
    /// Caso exista uma sala prévia, inicia o descarregamento da mesma.
    /// Determina a sala informada como a sala atual do jogo.
    /// Inicia o pré-carregamento da cena seguinte.
    /// </summary>
    /// <param name="sala"></param>
    public void SetSala(sala sala){

        // Descarrega a sala anterior :
        if (this.sala != null){
            StartCoroutine(UnloadSala(this.sala.gameObject.scene));
        }

        // Determina a sala informada como a sala atual :
        this.sala = sala;

        // Inicia o precarregamento da próxima sala :
        string proximaSala = sala.NomeProximaSala();
        if(proximaSala == string.Empty){
            return;
        }
        StartCoroutine(PreloadProximaSala(proximaSala));

    }

    #region Corotinas de carregamento

    IEnumerator PreloadProximaSala(string salaPCarregar){

        if(SceneUtility.GetBuildIndexByScenePath($"Scenes/{salaPCarregar}") == 0){
            Debug.Log("Proxima cena não está contida na build ou, não está com o nome correto.");
            yield break;
        }

        cenaProx = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);
        cenaProx.allowSceneActivation = false;
    }

    IEnumerator UnloadSala(Scene scene){
        AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
        
        yield return new WaitUntil(() => op.isDone);
        Debug.Log("Terminou de descarregar : " + scene.name);
    }

    #endregion

    #endregion
}
