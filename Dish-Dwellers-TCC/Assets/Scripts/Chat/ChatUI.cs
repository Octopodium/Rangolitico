using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour {
    public GameObject heaterPanel, anglerPanel;
    public Text heaterText, anglerText;

    void Start() {
        ChatController.instance.OnMensagemAngler += AtualizarAngler;
        ChatController.instance.OnMensagemOffAngler += () => AtualizarAngler();

        ChatController.instance.OnMensagemHeater += AtualizarHeater;
        ChatController.instance.OnMensagemOffHeater += () => AtualizarHeater();
    }

    public void AtualizarHeater(string texto = "") {
        heaterPanel.SetActive(texto != "");
        heaterText.text = texto;
    }

    public void AtualizarAngler(string texto = "") {
        anglerPanel.SetActive(texto != "");
        anglerText.text = texto;
    }
}