using System;
using UnityEngine;
using Unity.Services.Analytics;

public class SalaAnalyticsEvent : Unity.Services.Analytics.Event {
    public string sala {set { SetParameter("sala", value); }}
    public ModoDeJogo modoDeJogo {set { SetParameter("modoDeJogo", value.ToString()); }}
    public float tempoGasto {set { SetParameter("tempoGasto", value); }}
    public float tempoDesdeReset {set { SetParameter("tempoDesdeReset", value); }}
    public bool concluida {set { SetParameter("concluida", value); }}
    public bool usandoCheckpoint {set { SetParameter("usandoCheckpoint", value); }}
    public int mortes {set { SetParameter("mortes", value); }}

    public SalaAnalyticsEvent() : base("resumoSala") { }

}

public class SalaAnalytics {
    public string sala;
    public float tempoTotal;
    public float tempoReset;
    public ModoDeJogo modoDeJogo;
    public int mortes;
    public bool concluida;
    public bool checkpoint;

    public SalaAnalytics(string sala) {
        this.sala = sala;

        tempoTotal = 0f;
        tempoReset = 0f;
        mortes = 0;
        concluida = false;
        checkpoint = false;
    }

    public void FinalizarPartida(bool concluido = true) {
        concluida = concluido;
        modoDeJogo = GameManager.instance.modoDeJogo;

        var analytics = new SalaAnalyticsEvent {
            sala = sala,
            modoDeJogo = modoDeJogo,
            tempoGasto = tempoTotal,
            tempoDesdeReset = tempoReset,
            concluida = concluida,
            usandoCheckpoint = checkpoint,
            mortes = mortes
        };

       AnalyticsService.Instance.RecordEvent(analytics);
    }

    public void AtualizarTempo(float deltaTime) {
        tempoTotal += deltaTime;
        tempoReset += deltaTime;
    }

    public void RegistrarMorte() {
        mortes++;
        tempoReset = 0f;

        if (checkpointMarcado) checkpoint = true;
    }

    bool checkpointMarcado = false;
    public void MarcarCheckpoint(bool marcado = true) {
        checkpointMarcado = marcado;
        if (!marcado) checkpoint = false;
    }

}