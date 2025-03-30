using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Carregavel : MonoBehaviour, InteracaoCondicional {
    public UnityEvent onCarregado, onSolto;
    public System.Action<Carregador> OnCarregado, OnSolto; // Chamado quando o carregador carrega ou solta um objeto

    Rigidbody rb;
    bool _sendoCarregado = false;
    public bool sendoCarregado => _sendoCarregado;
    public Carregador carregador { get; private set; } // O carregador que está carregando o objeto, se houver

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public bool PodeInteragir(Player jogador) {
        return !jogador.carregador.estaCarregando && !_sendoCarregado;
    }

    public void Interagir(Player jogador) {
        Carregar(jogador.carregador);
    }
    
    public void Carregar(Carregador carregador) {
        if (carregador.estaCarregando || _sendoCarregado) return;
        carregador.Carregar(this);
        this.carregador = carregador;

        onCarregado.Invoke();
        OnCarregado?.Invoke(carregador);
    }

    public void HandleSendoCarregado() {
        _sendoCarregado = true;
        rb.isKinematic = true;
    }

    public void HandleSolto() {
        onSolto.Invoke();
        OnSolto?.Invoke(carregador);

        _sendoCarregado = false;
        rb.isKinematic = false;
        this.carregador = null;
    }
}
