using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        input = new Actions();
        input.Enable();
    }

    private void Start(){
        MostrarSalaFase();
    }

    #region Protótipo sistema de salas do Lima
    [Space(10)][Header("<color=green>Informações da sala :</color>")]
    [SerializeField] private bool loadingFinished;
    [SerializeField] private int sala, fase;
    [SerializeField] private string nomeDaSala;

    [Space(10)][Header("<color=green>Componentes da sala :</color>")]
    public TMP_Text displayNomeDaSala;

    /// <summary>
    /// Metodo para Debug do sistema de salas.
    /// </summary>
    private void MostrarSalaFase(){
        SalaAtual(out sala, out fase);
        Debug.Log($"Atualmente na <color=green>sala {sala}</color> na <color=green>fase {fase}</color>.");
    }

    /// <summary>
    /// Retorna a sala e a fase atual em numeros inteiros nos parâmetros out.
    /// </summary>
    /// <param name="sala"></param>
    /// <param name="fase"></param>
    /// <exception cref="Exception"></exception>
    public void SalaAtual(out int sala, out int fase){
        Scene cena = SceneManager.GetActiveScene();
        string log = string.Empty;
        string[] nome = cena.name.Split('-');

        if(Int32.TryParse(nome[0], out sala) && Int32.TryParse(nome[1], out fase)){
            log = $"Sala : {sala} Fase : {fase}";
        }
        else{
            throw new Exception($"Falha ao identificar o numero da fase. \n Nome da cena informado: {SceneManager.GetActiveScene().name} \n Nome da fase informado {nome[1]}");
        }

        try{
            nomeDaSala = nome[2];
            displayNomeDaSala.text = nomeDaSala;
            //Animação de display do nome.
        }
        catch{
            Debug.Log("Sala não possui nome.");
        }
    }

    IEnumerator PreCarregarProximaSala(){
        string nomeProximaSala = $"{sala +1}-{fase}";
        AsyncOperation op = SceneManager.LoadSceneAsync(nomeProximaSala, LoadSceneMode.Additive);
        yield return new WaitUntil (() => op.isDone);

    }

    #endregion
}
