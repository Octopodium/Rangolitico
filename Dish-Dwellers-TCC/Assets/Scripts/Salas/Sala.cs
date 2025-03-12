using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sala : MonoBehaviour
{
    [HideInInspector] public int sala, fase;
    [HideInInspector] public string nome;
    GameObject[] spawns = new GameObject[2];


    private void Awake(){
        SalaAtual(out sala, out fase);
    }

    private void OnEnable(){
        GameManager.instance.sala = this;

        if(spawns.Length < 2){
            spawns = GameObject.FindGameObjectsWithTag("Spawn");
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Tenta colocar cada jogador encontrado em um spawn diferente da sala.
        for( int i = 0; i < players.Length; i++){
            players[i].transform.position = spawns[i].transform.position;
        }
    }

    // Utiliza o nome da cena em que o objeto está para determinar o numero da sala e da fase correspondete.
    public void SalaAtual(out int sala, out int fase){
        // Separa o nome da cena em (idealmente) 3 partes,separadas por '-', seguindo o modelo "sala-fase-nome".
        Scene cena = SceneManager.GetActiveScene();
        string log = string.Empty;
        string[] nome = cena.name.Split('-');

        // Tenta extrair do nome da cena os numeros de sala e fase.
        // Caso o processo falhe, assume que a sala é uma sala invalida com valor de -1 em ambos os campos.
        if(Int32.TryParse(nome[0], out sala) && Int32.TryParse(nome[1], out fase)){
            log = $"Sala : {sala} Fase : {fase}";
        }
        else{
            sala = -1;
            fase = -1;
            log = $"Falha ao identificar o numero da fase. \n Nome da cena informado: {SceneManager.GetActiveScene().name} \n Nome da fase informado {nome[1]}";
        }

        Debug.Log(log);

        // Procura por um nome no terceiro elemento do arranjo. Faz o display caso encontre.
        try{
            this.nome = nome[2];
            //Animação de display do nome.
        }
        catch{
            Debug.Log("Sala não possui nome.");
        }
    }
}
