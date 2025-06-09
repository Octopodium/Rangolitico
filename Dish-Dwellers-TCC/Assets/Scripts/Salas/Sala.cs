using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class sala : MonoBehaviour{
    
    [Space(10)][Header("<color=yellow>Referências manuais: </color>")][Space(10)]
    public Transform[] spawnPoints = new Transform[2];
    public List<OnTriggerEvent> triggers = new List<OnTriggerEvent>();
    public List<IResetavel> resetaveis = new List<IResetavel>();
    public UnityEvent onResetSala;

    [HideInInspector]public int nSala, nFase;

    private void Start() {

        GetNomeDaSala();
        PosicionarJogador();
        GameManager.instance.SetSala(this);
        Debug.Log("Lista formada");
        ResetSala();
    }

    /// <summary>
    /// Reinicia a sala para suas condições iniciais.
    /// </summary>
    public void ResetSala(){

        // Reposiciona o jogador na sua posição inicial, e restaura sua vida.
        PosicionarJogador();
        foreach( var player in GameManager.instance.jogadores){
            player.Resetar();
        }

        // Reativa todos os triggers da sala.
        foreach(OnTriggerEvent trigger in triggers){
            trigger.gameObject.SetActive(true);
        }

        onResetSala?.Invoke();
        foreach(var data in resetaveis){ 
            data.OnReset();
        }
        
    }

    // Separa o nome da cena para encontrar o numero da fase e da sala.
    private void GetNomeDaSala(){
        // Separa o nome da cena em partes separadas por '-', seguindo o modelo "sala-fase".
        string[] nome = gameObject.scene.name.Split('-');

        if(int.TryParse(nome[0], out nSala) && int.TryParse(nome[1], out nFase)){
            Debug.Log($"Sala : {nSala} Fase : {nFase}");
        }
        else{
            nSala = -1;
            nFase = -1;
            Debug.Log($"Falha ao identificar o numero da fase. \n Nome da cena informado: {SceneManager.GetActiveScene().name} \n Nome da fase informado {nome[1]}");
        }
    }

    // Utiliza o numero da sala/fase atual para descobrir o nome da proxima cena a ser carregada.
    public string NomeProximaSala(){
        string nome = $"{nSala + 1}-{nFase}";
        if(SceneUtility.GetBuildIndexByScenePath("Scenes/" + nome) < 0){

            nome = $"1-{nFase + 1}";

            if(SceneUtility.GetBuildIndexByScenePath("Scene/" + nome) < 0){
                Debug.Log("<color=yellow>Proxima sala não foi encontrada, não será possivel prosseguir.");
                return string.Empty;
            }

        }

        return nome;

    }

    /// <summary>
    /// Posiciona os jogadores nos spawnPoints da sala.
    /// </summary>
    public void PosicionarJogador(){
        // Tenta colocar cada jogador encontrado em um spawn diferente da sala.
        List<Player> players = GameManager.instance.jogadores;

        for( int i = 0; i < players.Count; i++){
            players[i].Teletransportar(spawnPoints[i].position);
            players[i].gameObject.SetActive(true);
        }
    }

    /// </summary>
    /// Retorna o nome da sala atual, no formato "sala-fase".
    /// <summary>
    public string GetNome() {
        return $"{nSala}-{nFase}";
    }
}
