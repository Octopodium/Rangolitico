using System;
using UnityEngine;
using Unity.Services.Analytics;

public class MorteAnalyticsEvent : Unity.Services.Analytics.Event {
    public string sala {set { SetParameter("sala", value); }}
    public ModoDeJogo modoDeJogo {set { SetParameter("modoDeJogo", value.ToString()); }}
    public string causa {set { SetParameter("causa", value); }}
    public float tempoDesdeReset {set { SetParameter("tempoDesdeReset", value); }}
    public bool usandoCheckpoint {set { SetParameter("usandoCheckpoint", value); }}
    public float posX {set { SetParameter("posX", value); }}
    public float posY {set { SetParameter("posY", value); }}
    public float posZ {set { SetParameter("posZ", value); }}
    public Vector3 pos {set {
        SetParameter("posX", value.x);
        SetParameter("posY", value.y);
        SetParameter("posZ", value.z);
    }}
    public string quadrante {set { SetParameter("quadrante", value); }}

    public MorteAnalyticsEvent() : base("morte") { }
}

public class MorteAnalytics {
    public string sala;
    public ModoDeJogo modoDeJogo;
    public string causa;
    public float tempoDesdeReset;
    public bool usandoCheckpoint;
    public Vector3 pos;
    public string quadrante;

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
            quadrante = quadrante
        };

       AnalyticsService.Instance.RecordEvent(analytics);
    }

}