using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

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

        StartCoroutine(OnEntraPrimeiraSala());
    }


    #region Sistema de salas do Lima
    [Space(10)][Header("<color=green>Informações da sala :</color>")]
    
    [SerializeField] private bool descarregando, carregando;
    private sala salaProx, salaAnt;

    
    // Chamado toda vez que o jogador passa de Sala.
    public void PassaDeSala(){
        StartCoroutine(PassarDeSala());
    }

    // Utilizado para permitir que as prorprias salas se estabeleçam como a proxima sala.
    public void SetProximaSala(sala sala){
        salaProx = sala;
    }

    #region Coroutinas de carregamento

    // Rotina chamada somente ao iniciar o jogo
    IEnumerator OnEntraPrimeiraSala(){
        // Espera a sala ser encontrada para continuar e trata o caso em que a sala não é encontrada.
        float timer = 0;

        while(salaProx == null){
            timer += Time.deltaTime;
            if(timer >= 3){
                Debug.Log("<color=yellow>Sala não foi encontrada. O jogo continuará normalmente, porem, o sistema de salas pode não funcionar como esperado.</color>");
                yield break;
            }
            yield return null;
        }
        salaAnt = salaProx;
        StartCoroutine(PreloadProximaSala());
    }

    // Realiza todo o processo de desativar e descarregar a sala anterior, e ativar a proxima sala.
    IEnumerator PassarDeSala(){
        yield return new WaitUntil(() => descarregando == false && carregando == false);
        salaAnt.Desativar();
        salaProx.Ativar();
        StartCoroutine(UnloadSala());
        StartCoroutine(PreloadProximaSala());
    }

    IEnumerator PreloadProximaSala(){
        AsyncOperation op;
        string salaPCarregar;

        yield return new WaitWhile(() => descarregando);
        carregando = true;

        try{
            salaPCarregar = $"{salaAnt.nSala + 1}-{salaAnt.nFase}";
            op = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);
        }
        catch{
            salaPCarregar = $"01-{salaAnt.nFase + 1}";
            op = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);
        }

        yield return new WaitUntil (() => {
            if(op != null) return op.isDone;
            else return true;
        });
        carregando = false;
    }

    IEnumerator UnloadSala(){
        descarregando = true;
        
        AsyncOperation op = SceneManager.UnloadSceneAsync(salaAnt.gameObject.scene);
        yield return new WaitUntil(() => op.isDone);

        salaAnt = salaProx;
        descarregando = false;
    }
    #endregion

    #endregion
}
