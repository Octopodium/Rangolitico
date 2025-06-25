using UnityEngine;
using System;

[Serializable]
public class Mensagem {
    public string text;
    public float tempo;
    public bool ativo => tempo > 0f;
}

public class ChatController : MonoBehaviour, SincronizaMetodo {
    public static ChatController instance;

    public float tempoBase;
    public Mensagem heaterMsg, anglerMsg;

    public Action<string> OnMensagemHeater, OnMensagemAngler;
    public Action OnMensagemOffHeater, OnMensagemOffAngler;

    void Awake() {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    void FixedUpdate() {
        if (heaterMsg.ativo) {
            heaterMsg.tempo -= Time.fixedDeltaTime;
            if (!heaterMsg.ativo) OnMensagemOffHeater.Invoke();
        }

        if (anglerMsg.ativo) {
            anglerMsg.tempo -= Time.fixedDeltaTime;
            if (!anglerMsg.ativo) OnMensagemOffAngler.Invoke();
        }
    }

    public void MandarMensagem(string text, Player player) {
        MandarMensagem(text, player.personagem);
    }

    [Sincronizar]
    public void MandarMensagem(string text, QualPersonagem personagem) {
        if (string.IsNullOrEmpty(text)) return;
        
        gameObject.Sincronizar(text, personagem);

        Mensagem msg = PegarMensagem(personagem);
        msg.text = text;
        msg.tempo = tempoBase;

        PegarOnMensagem(personagem).Invoke(text);
    }

    public Mensagem PegarMensagem(QualPersonagem qualPersonagem) {
        switch (qualPersonagem) {
            case QualPersonagem.Heater:
                return heaterMsg;
            case QualPersonagem.Angler:
                return anglerMsg;
        }

        return heaterMsg;
    }

    public Action<string> PegarOnMensagem(QualPersonagem qualPersonagem) {
        switch (qualPersonagem) {
            case QualPersonagem.Heater:
                return OnMensagemHeater;
            case QualPersonagem.Angler:
                return OnMensagemAngler;
        }

        return OnMensagemHeater;
    }

    public Action PegarOnMensagemOff(QualPersonagem qualPersonagem) {
        switch (qualPersonagem) {
            case QualPersonagem.Heater:
                return OnMensagemOffHeater;
            case QualPersonagem.Angler:
                return OnMensagemOffAngler;
        }

        return OnMensagemOffHeater;
    }

    public bool IsEmote(string mensagem) {
        return mensagem.StartsWith(":");
    }

    public Sprite GetEmoteSprite(string mensagem) {
        if (!IsEmote(mensagem)) return null;
        string emoteName = mensagem.Substring(1); // Remove o prefixo ":"
        return Resources.Load<Sprite>("Emotes/" + emoteName);
    }
}
