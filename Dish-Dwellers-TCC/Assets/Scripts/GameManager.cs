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
    
    [SerializeField] private bool descarregando;
    [SerializeField] private bool carregando;
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

        float timer = 0;

        // Aguarda que algum MonoBehaviour Sala chame o SetProximaSala(), caso demore mais de 3 segundos, o processo é encerrado.
        while(salaProx == null){

            timer += Time.deltaTime;

            if(timer >= 3){
                Debug.Log("<color=yellow>Sala não foi encontrada. O jogo continuará normalmente, porem, o sistema de salas pode não funcionar como esperado.</color>");
                yield break;
            }

            yield return null;
        }

        
        salaAnt = salaProx;
        Debug.Log($"<color=green> Sala econtrada com Sucesso!</color>\n<color=yellow> Numero da sala : {salaProx.nSala}\t Numero da fase : {salaProx.nFase} ");

        StartCoroutine(PreloadProximaSala());
    }

    // Realiza todo o processo de desativar e descarregar a sala anterior, e ativar a proxima sala.
    IEnumerator PassarDeSala(){

        // Caso descarregamnento ou carregamento das salas esteja sendo realizado, aguarda pela finalização do processo.
        if(descarregando || carregando){
            
        }
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

        salaPCarregar = $"{salaAnt.nSala + 1}-{salaAnt.nFase}";
        op = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);


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
