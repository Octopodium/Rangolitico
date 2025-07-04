using System;
using UnityEngine;
using Unity.Services.Analytics;

public class MorteAnalyticsEvent : Unity.Services.Analytics.Event {
    public string sala {set { SetParameter("sala", value); }}
    public ModoDeJogo modoDeJogo {set { SetParameter("modoDeJogo", value.ToString()); }}
    public string causa {set { SetParameter("causa", value); }}
    public float tempoDesdeReset {set { SetParameter("tempoDesdeReset", value); }}
    public bool usandoCheckpoint {set { SetParameter("usandoCheckpoint", value); }}
    public Vector3 pos {set {
        float x = Mathf.Round(value.x * 1000f) / 1000f;
        float y = Mathf.Round(value.y * 1000f) / 1000f;
        float z = Mathf.Round(value.z * 1000f) / 1000f;
        SetParameter("posicao", $"{x},{y},{z}");
    }}
    public string quad {set { SetParameter("quad", value); }}

    public MorteAnalyticsEvent() : base("morte") { }
}

public class MorteAnalytics {
    public string sala;
    public ModoDeJogo modoDeJogo;
    public string causa;
    public float tempoDesdeReset;
    public bool usandoCheckpoint;
    public Vector3 pos;
    public string quad;

    public MorteAnalytics() { }


    public void Registrar() {
        modoDeJogo = GameManager.instance.modoDeJogo;

        var analytics = new MorteAnalyticsEvent {
            sala = sala,
            modoDeJogo = modoDeJogo,
            causa = causa,
            tempoDesdeReset = tempoDesdeReset,
            usandoCheckpoint = usandoCheckpoint,
            pos = pos,
            quad = quad
        };

       
        try { AnalyticsService.Instance.RecordEvent(analytics); } catch (Exception e) { Debug.LogError($"Erro ao registrar evento de morte: {e.Message}"); }
    }

}