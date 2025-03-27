using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public Actions input;


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

        cenaAtual = SceneManager.GetActiveScene();
    }


    #region Sistema de salas do Lima
    [Space(10)][Header("<color=green>Informações da sala :</color>")]
    
    [SerializeField] private bool descarregando;
    [SerializeField] private bool carregando;
    private bool passarDeSala = false;
    private Scene cenaAtual, cenaAnt;
    private AsyncOperation cenaProx;
    private sala sala;

    
    // Chamado toda vez que o jogador passa de Sala.
    public void PassaDeSala(){
        cenaProx.allowSceneActivation = true;
    }

    public void SetSala(sala sala){
        if (this.sala != null){
            StartCoroutine(UnloadSala(this.sala.gameObject.scene));
        }

        this.sala = sala;
        string proximaSala = sala.NomeProximaSala();
        if(proximaSala == string.Empty) return;
        
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
