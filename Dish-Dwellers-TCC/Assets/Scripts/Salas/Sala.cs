using UnityEngine;
using UnityEngine.SceneManagement;

public class sala : MonoBehaviour
{
    GameObject[] spawns = new GameObject[2];
    public int nSala, nFase;
    GameObject salaObj;
    public bool setupPronto;


    private void Start(){
        salaObj = transform.GetChild(0).gameObject;
        GetNomeDaSala();
        GameManager.instance.SetProximaSala(this);
    }

    public void Ativar(){
        if(salaObj.activeInHierarchy){ 
            Debug.Log("<color=red> Sala já está ativada! </color>");
        }
        else{
            salaObj.SetActive(true);
            PosicionarJogador();
        }
    }


    public void Desativar(){

    }

    private void GetNomeDaSala(){
        // Separa o nome da cena em (idealmente) 3 partes,separadas por '-', seguindo o modelo "sala-fase-nome".
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

    private void PosicionarJogador(){
        if(spawns.Length < 2){
            spawns = GameObject.FindGameObjectsWithTag("Spawn");
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Tenta colocar cada jogador encontrado em um spawn diferente da sala.
        for( int i = 0; i < players.Length; i++){
            //players[i].transform.position = spawns[i].transform.position;
        }
    }
}
