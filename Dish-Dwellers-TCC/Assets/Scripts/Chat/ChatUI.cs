using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour {
    public GameObject heaterPanel, anglerPanel;
    public Image heaterEmote, anglerEmote;

    void Start() {
        ChatController.instance.OnMensagemAngler += AtualizarAngler;
        ChatController.instance.OnMensagemOffAngler += AtualizarAngler;

        ChatController.instance.OnMensagemHeater += AtualizarHeater;
        ChatController.instance.OnMensagemOffHeater += AtualizarHeater;
    }

    void OnDestroy() {
        ChatController.instance.OnMensagemAngler -= AtualizarAngler;
        ChatController.instance.OnMensagemOffAngler -= AtualizarAngler;

        ChatController.instance.OnMensagemHeater -= AtualizarHeater;
        ChatController.instance.OnMensagemOffHeater -= AtualizarHeater;
    }

    public void AtualizarHeater() {
        AtualizarHeater("");
    }

    public void AtualizarHeater(string texto = "") {
        if (texto != "" && ChatController.instance.IsEmote(texto)) {
            heaterEmote.sprite = ChatController.instance.GetEmoteSprite(texto);
            heaterPanel.SetActive(true);
        } else {
            heaterEmote.sprite = null;
            heaterPanel.SetActive(false);
        }
    }
    
    public void AtualizarAngler() {
        AtualizarAngler("");
    }
    public void AtualizarAngler(string texto = "") {
        if (texto != "" && ChatController.instance.IsEmote(texto)) {
            anglerEmote.sprite = ChatController.instance.GetEmoteSprite(texto);
            anglerPanel.SetActive(true);
        } else {
            anglerEmote.sprite = null;
            anglerPanel.SetActive(false);
        }
    }
}