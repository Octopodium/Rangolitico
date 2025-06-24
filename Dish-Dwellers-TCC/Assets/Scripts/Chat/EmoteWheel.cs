using UnityEngine;
using UnityEngine.UI;

public class EmoteWheel : MonoBehaviour {
    public Player player;
    public RadialLayoutUI wheel;

    public float deadzone = 0.1f;

    void Start() {
        wheel.gameObject.SetActive(false);
        player.onEmoteWheel += ShowEmoteWheel;

        personagemBalao = player.personagem;
        
        if (personagemBalao == QualPersonagem.Heater) {
            ChatController.instance.OnMensagemHeater += MostrarBalao;
            ChatController.instance.OnMensagemOffHeater += EsconderBalao;
        } else if (personagemBalao == QualPersonagem.Angler) {
            ChatController.instance.OnMensagemAngler += MostrarBalao;
            ChatController.instance.OnMensagemOffAngler += EsconderBalao;
        }
    }

    void OnDestroy() {
        player.onEmoteWheel -= ShowEmoteWheel;

        if (personagemBalao == QualPersonagem.Heater) {
            ChatController.instance.OnMensagemHeater -= MostrarBalao;
            ChatController.instance.OnMensagemOffHeater -= EsconderBalao;
        } else if (personagemBalao == QualPersonagem.Angler) {
            ChatController.instance.OnMensagemAngler -= MostrarBalao;
            ChatController.instance.OnMensagemOffAngler -= EsconderBalao;
        }
    }

    public void ShowEmoteWheel(bool show) {
        wheel.gameObject.SetActive(show);

        if (show) balao.SetActive(false);
        else if (mostrandoBalao) balao.SetActive(true);

        if (!show && selectedEmote != null) {
            string emoteName = ":" + selectedEmote.emoteName; // Prefixo ":" para emotes
            ChatController.instance.MandarMensagem(emoteName, player);
            
            selectedEmote.Unselected();
            selectedEmote = null;
        }
    }

    EmoteButton selectedEmote;
    Vector2 inputMira;
    void FixedUpdate() {
        if (!wheel.gameObject.activeSelf) return;
        inputMira = player.playerInput.currentActionMap["Aim"].ReadValue<Vector2>();

        if (inputMira.magnitude < deadzone) {
            Unselect();
            return;
        }

        GameObject currentSelectedEmote = wheel.GetChild(inputMira.normalized);
        if (currentSelectedEmote != selectedEmote) {
            EmoteButton emoteButton = currentSelectedEmote.GetComponent<EmoteButton>();
            if (emoteButton == null) return;
            Unselect();
            emoteButton.Selected();
            selectedEmote = emoteButton;
        } else {
            Unselect();
        }

    }

    void Unselect() {
        if (selectedEmote == null) return;
        selectedEmote.Unselected();
        selectedEmote = null;
    }


    #region Mostrar Balao

    [Header("Bolão Ingame")]
    public GameObject balao;
    public Image emote;
    QualPersonagem personagemBalao;
    bool mostrandoBalao = false;

    public void MostrarBalao(string mensagem) {
        mostrandoBalao = true;

        if (mensagem != "" && ChatController.instance.IsEmote(mensagem)) {
            emote.sprite = ChatController.instance.GetEmoteSprite(mensagem);
            balao.SetActive(true);
        } else {
            mostrandoBalao = false;
            emote.sprite = null;
            balao.SetActive(false);
        }
    }

    public void EsconderBalao() {
        mostrandoBalao = false;
        balao.SetActive(false);
    }

    #endregion
}
