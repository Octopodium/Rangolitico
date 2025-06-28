using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour {
    public static AnalyticsManager instance;

    public PartidaAnalytics partida = null;
    public SalaAnalytics sala = null;


    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    async void Start() {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
        Debug.Log("Unity Services initialized successfully.");
    }

    void FixedUpdate() {
        if (partida != null) {
            partida.AtualizarTempo(Time.fixedDeltaTime);
        }

        if (sala != null) {
            sala.AtualizarTempo(Time.fixedDeltaTime);
        }
    }


    public void ComecarPartida() {
        partida = new PartidaAnalytics();
    }

    public void FinalizarPartida(bool concluido) {
        if (partida != null) {
            partida.FinalizarPartida(concluido);
            partida = null;
        }
    }

    public void ComecarSala(string salaId) {
        if (sala != null) {
            sala.FinalizarPartida();
        }

        if (partida != null) {
            partida.AtualizarSala(salaId);
        }

        sala = new SalaAnalytics(salaId);
    }

    public void FinalizarSala(bool concluido = true) {
        if (sala != null) {
            sala.FinalizarPartida(concluido);
            sala = null;
        }
    }

    public void RegistrarMorte(string causa, Vector3 pos) {
        bool checkpoint = false;
        string nomeSala = "";
        float tempoDesdeReset = -1f;
        string quadrante = GetQuadrante(pos);

        if (sala != null) {
            checkpoint = sala.checkpoint;
            nomeSala = sala.sala;
            tempoDesdeReset = sala.tempoReset;
            
            sala.RegistrarMorte();
        }

        if (partida != null) partida.RegistrarMorte();

        var analytics = new MorteAnalytics {
            sala = nomeSala,
            modoDeJogo = GameManager.instance.modoDeJogo,
            causa = causa,
            tempoDesdeReset = tempoDesdeReset,
            usandoCheckpoint = checkpoint,
            pos = pos,
            quadrante = quadrante
        };

        analytics.Registrar();
    }

    public string GetQuadrante(Vector3 pos) {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);
        int z = Mathf.RoundToInt(pos.z);

        // ignorar a quantidade de quadrantes

        return $"{x},{y},{z}";
    }
}
