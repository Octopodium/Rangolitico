using System;
using UnityEngine;
using Unity.Services.Analytics;

public class PartidaAnalyticsEvent : Unity.Services.Analytics.Event {
    public string ultimaSala  {set { SetParameter("ultimaSala", value); }}
    public ModoDeJogo modoDeJogo {set { SetParameter("modoDeJogo", value.ToString()); }}
    public float tempoGasto {set { SetParameter("tempoGasto", value); }}
    public bool concluida {set { SetParameter("concluida", value); }}
    public int mortes {set { SetParameter("mortes", value); }}

    public PartidaAnalyticsEvent() : base("resumoPartida") { }
}


public class PartidaAnalytics {
    public float tempo;
    public ModoDeJogo modoDeJogo;
    public int mortes;
    public bool concluida;
    public string ultimaSala;

    public PartidaAnalytics() {
        tempo = 0f;
        mortes = 0;
        concluida = false;
    }

    public void FinalizarPartida(bool concluido = true) {
        concluida = concluido;
        modoDeJogo = GameManager.instance != null ? GameManager.instance.modoDeJogo : ModoDeJogo.INDEFINIDO;

        var analytics = new PartidaAnalyticsEvent {
            ultimaSala = ultimaSala,
            modoDeJogo = modoDeJogo,
            tempoGasto = tempo,
            concluida = concluida,
            mortes = mortes
        };

        try { AnalyticsService.Instance.RecordEvent(analytics); } catch (Exception e) { Debug.LogError($"Erro ao registrar evento de partida: {e.Message}"); }
    }

    public void AtualizarTempo(float deltaTime) {
        tempo += deltaTime;
    }

    public void RegistrarMorte() {
        mortes++;
    }

    public void AtualizarSala(string sala) {
        ultimaSala = sala;
    }
}