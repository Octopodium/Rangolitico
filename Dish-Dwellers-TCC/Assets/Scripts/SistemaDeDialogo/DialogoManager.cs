using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class DialogoManager : MonoBehaviour
{
    [SerializeField] private TMP_Text textoDialogo;
    [TextArea(3,3)]
    public string[] linhas;

    private int index = -1;

    void IniciaDialogo(){
        //Caso seja um Panel, ativa o panel, caso seja uma cena, iniciar no Start()
        ProximaLinha();
    }

    public void ProximaLinha(){
        if(index + 1 == linhas.Length){
            FinalizaDialogo();
            return;
        }

        index++;
        string linha = linhas[index];

        StartCoroutine(EscreveLinha(linha));
    }

    IEnumerator EscreveLinha(string linha){
        textoDialogo.text = "";
        foreach(char letra in linha.ToCharArray()){
            textoDialogo.text += letra;
            yield return null;
        }
    }

    void FinalizaDialogo(){
        Debug.Log("Dialogo terminado");
        index = -1;
        //Carrega proxima zona
        //No momento, se continuar clicando em proximo, o dialogo se repete
    }
}
